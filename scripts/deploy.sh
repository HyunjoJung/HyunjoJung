#!/bin/bash

# Portfolio 배포 스크립트 (로컬 빌드 방식)
# 사용법: ./deploy.sh

set -e  # 오류 발생 시 스크립트 중단

# 설정
SERVER_USER="joe"
SERVER_HOST="192.168.0.8"
SERVER_PORT="22"
DEPLOY_PATH="/var/www/portfolio"

echo "=========================================="
echo "🚀 Portfolio 배포 시작"
echo "=========================================="

# 1. 프로젝트 파일 서버로 전송
echo ""
echo "[1/3] 프로젝트 파일 전송 중..."

# 임시 디렉토리 생성 및 필요한 파일만 복사
TEMP_DIR=$(mktemp -d)
echo "📦 임시 디렉토리: $TEMP_DIR"

# 필요한 파일 복사 (.gitignore 기준으로 제외)
rsync -av --exclude-from='.gitignore' \
    --exclude='.git' \
    --exclude='scripts' \
    --exclude='.vs' \
    --exclude='.vscode' \
    --exclude='bin' \
    --exclude='obj' \
    ./ "$TEMP_DIR/"

# Production 설정 파일 복사 (gitignore에 있지만 배포에 필요)
if [ -f "appsettings.Production.json" ]; then
    echo "🔐 appsettings.Production.json 복사 중..."
    cp appsettings.Production.json "$TEMP_DIR/"
fi

# 서버로 전송
echo "📤 서버로 파일 전송 중..."
sshpass -p "1234" rsync -avz --delete -e "ssh -p $SERVER_PORT -o StrictHostKeyChecking=no" \
    "$TEMP_DIR/" \
    "$SERVER_USER@$SERVER_HOST:$DEPLOY_PATH/"

# 임시 디렉토리 정리
rm -rf "$TEMP_DIR"

if [ $? -ne 0 ]; then
    echo "❌ 파일 전송 실패"
    exit 1
fi

echo "✅ 파일 전송 완료"

# 2. 서버에서 빌드 및 재시작
echo ""
echo "[2/3] 서버에서 Docker 빌드 및 재시작 중..."
sshpass -p "1234" ssh -p "$SERVER_PORT" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_HOST" << EOF
    cd $DEPLOY_PATH

    # 기존 컨테이너 중지 및 제거
    echo "🛑 기존 컨테이너 중지 중..."
    docker-compose down || true

    # 이전 이미지 정리 (선택적)
    echo "🧹 이전 이미지 정리 중..."
    docker image prune -f

    # 새 이미지 빌드
    echo "🔨 Docker 이미지 빌드 중..."
    docker-compose build --no-cache

    # 새 컨테이너 시작
    echo "🚀 새 컨테이너 시작 중..."
    docker-compose up -d

    # 컨테이너 상태 확인
    sleep 3
    docker-compose ps

    # 로그 확인 (최근 20줄)
    echo ""
    echo "📋 최근 로그:"
    docker-compose logs --tail=20
EOF

if [ $? -ne 0 ]; then
    echo "❌ 배포 실패"
    exit 1
fi

# 3. 기본 연결 확인
echo ""
echo "[3/3] 서비스 연결 확인 중..."
sleep 2

CONNECTION_CHECK=$(sshpass -p "1234" ssh -p "$SERVER_PORT" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_HOST" \
    "curl -s -o /dev/null -w '%{http_code}' http://localhost:5051 || echo 'FAILED'")

if [[ "$CONNECTION_CHECK" == "FAILED" ]]; then
    echo "⚠️  연결 확인 실패 (서비스가 아직 시작 중일 수 있습니다)"
elif [[ "$CONNECTION_CHECK" == "200" ]]; then
    echo "✅ 서비스 정상 응답 (HTTP $CONNECTION_CHECK)"
else
    echo "⚠️  예상치 못한 응답 (HTTP $CONNECTION_CHECK)"
fi

echo ""
echo "=========================================="
echo "✅ 배포 완료!"
echo "=========================================="
echo ""
echo "🌐 사이트: https://hyunjo.uk/"
echo ""
echo "📊 서버 상태 확인:"
echo "  ssh joe@192.168.0.8 'cd $DEPLOY_PATH && docker-compose ps'"
echo ""
echo "📋 로그 확인:"
echo "  ssh joe@192.168.0.8 'cd $DEPLOY_PATH && docker-compose logs -f'"
echo ""
echo "🔄 재시작:"
echo "  ssh joe@192.168.0.8 'cd $DEPLOY_PATH && docker-compose restart'"
echo ""
