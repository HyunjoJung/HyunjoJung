---
title: Worlabel - Vite + Konva + YOLOv8로 구축한 자동 라벨링 플랫폼
description: Presigned URL (18분 → 90초)과 react-window 가상화로 고성능 이미지 라벨링 도구 구축 - 10,000장 이상의 이미지를 인터랙티브 캔버스 주석으로 최적화
date: 2024-10-11
tags:
  - React
  - Vite
  - Zustand
  - Konva
  - YOLOv8
  - Performance
category: Frontend
featured: true
---

# Worlabel: 자동화된 이미지 라벨링 플랫폼

> **일과 삶의 균형을 위한 자동화**
> "워라벨" - 지루한 수동 라벨링에서 벗어나 더 나은 모델 학습에 집중하세요

## 프로젝트 개요

Worlabel은 머신러닝 프로젝트의 이미지 주석 작업 시간을 대폭 단축하는 웹 기반 자동 라벨링 서비스입니다. 이 플랫폼은 반복적인 개선 사이클을 가능하게 합니다:

1. **수동 라벨링** → 캔버스 드로잉 도구로 작은 데이터셋 주석
2. **모델 학습** → 라벨링된 데이터로 YOLOv8 학습
3. **자동 라벨링** → 모델이 나머지 이미지의 라벨 예측
4. **검토 및 개선** → 사용자가 예측을 수정하고 재학습

이 지속적인 루프는 수작업을 절약하면서 점점 더 정확한 모델을 생산합니다.

**개발 기간**: 2024년 8월 19일 - 10월 11일 (8주)

**팀**: 개발자 6명 (프론트엔드 2, 백엔드 2, AI 1, 인프라 1)

## 나의 역할: 프론트엔드 개발자

두 명의 프론트엔드 개발자 중 한 명으로서 다음에 집중했습니다:

1. **성능 최적화** - Presigned URL 업로드 아키텍처 (18분 → 90초)
2. **가상화 렌더링** - 1000+ 이미지 목록을 위한 react-window
3. **캔버스 주석** - 세그멘테이션을 위한 Konva 기반 드로잉 도구
4. **상태 관리** - 캔버스 및 이미지 선택을 위한 Zustand 스토어

## 기술 스택

### 프론트엔드
- **Vite 5.3.1** - HMR을 갖춘 초고속 빌드 도구
- **React 18.3.1** - 컴포넌트 기반 UI
- **Zustand 4.5.5** - 경량 상태 관리 (3KB)
- **react-konva 18.2.10** - 캔버스 기반 주석 도구
- **react-window 1.8.10** - 가상화 목록 렌더링
- **TanStack Query 5.52.1** - 서버 상태 관리
- **React Hook Form + Zod** - 폼 검증
- **TailwindCSS** - 유틸리티 우선 스타일링

### 백엔드
- **Spring Boot** - RESTful API
- **JPA** - MySQL용 ORM
- **FastAPI** - AI 추론 서버

### AI/ML
- **YOLOv8 (Ultralytics)** - 객체 탐지 및 인스턴스 세그멘테이션
- **PyTorch** - 딥러닝 프레임워크

### 인프라
- **MySQL** - 관계형 데이터베이스
- **Redis** - 캐싱 레이어
- **Amazon S3** - 이미지 객체 스토리지
- **Docker + Jenkins** - CI/CD
- **GPU 서버** - CUDA 학습 인프라

## 주요 기능 및 구현

### 1. Presigned URL 업로드: 92% 시간 단축

**문제**: 전통적인 서버 매개 업로드로 10,000장의 이미지를 업로드하는 데 18분이 소요되었습니다.

#### 최적화된 접근: 직접 S3 업로드

```typescript
// ✅ Presigned URL 접근 - S3로 직접 업로드
export async function uploadImagePresigned(
  memberId: number,
  projectId: number,
  folderId: number,
  files: File[],
  processCallback: (index: number) => void
) {
  // 1단계: 서버에 메타데이터만 전송 (경량)
  const imageMetaList = files.map((file: File, index: number) => ({
    id: index,
    fileName: file.name,
  }));

  // 2단계: 서버에서 presigned URL 수신
  const { data: presignedUrlList } = await api.post(
    `/projects/${projectId}/folders/${folderId}/images/presigned`,
    imageMetaList,
    { params: { memberId } }
  );

  // 3단계: S3로 직접 업로드 (서버 우회)
  for (const presignedUrlInfo of presignedUrlList) {
    const file = files[presignedUrlInfo.id];
    await axios.put(presignedUrlInfo.presignedUrl, file, {
      headers: { 'Content-Type': file.type },
    });
  }
}
```

**결과**:
- **업로드 시간**: 18분 → **90초** (92% 빠름)
- **서버 대역폭**: ~95% 감소 (메타데이터만, 이미지 데이터 없음)
- **확장성**: S3가 로드를 처리, 서버는 서명만 생성
- **비용**: EC2 데이터 전송 비용 감소

### 2. react-window를 사용한 가상화 목록 렌더링

**문제**: 1000+ 이미지를 썸네일과 함께 렌더링하면 스크롤이 느려지고 초기 로드 시간이 길어졌습니다.

```tsx
// ✅ 한 번에 표시되는 행만 렌더링 (~10-15개)
import { FixedSizeList } from 'react-window';

export default function ImageSelection({ projectId, selectedImages }: Props) {
  const { allSavedImages } = useRecursiveSavedImages(projectId, 0);

  const Row = useMemo(() => {
    return React.memo(({ index, style }: { index: number; style: React.CSSProperties }) => {
      const image = allSavedImages[index];
      return (
        <div key={image.id} style={style} className="relative flex items-center">
          <span>{image.imageTitle}</span>
          <Button onClick={() => handleImageSelect(image.id)}>
            {selectedImages.includes(image.id) ? '해제' : '선택'}
          </Button>
        </div>
      );
    });
  }, [allSavedImages, selectedImages]);

  return (
    <FixedSizeList
      height={260}
      itemCount={allSavedImages.length}
      itemSize={80}
      width="100%"
    >
      {Row}
    </FixedSizeList>
  );
}
```

**성능 향상**:
- **초기 렌더**: 5-10초 → <100ms
- **스크롤 FPS**: 15-20 FPS → 60 FPS (매우 부드러움)
- **메모리 사용량**: ~500MB → ~8MB (98% 감소)

### 3. react-konva를 사용한 캔버스 기반 주석

객체 탐지/세그멘테이션 라벨을 위한 인터랙티브 폴리곤 및 사각형 드로잉.

```tsx
import { Stage, Layer, Image, Line, Circle, Rect } from 'react-konva';

export default function ImageCanvas() {
  const { image, labels, drawState } = useCanvasStore();
  const [polygonPoints, setPolygonPoints] = useState<[number, number][]>([]);

  return (
    <Stage
      width={800}
      height={600}
      draggable={true}
      onWheel={handleZoom}
      onMouseDown={handleClick}
    >
      <Layer>
        <Image image={image} />
      </Layer>
      <Layer>
        {labels.map((label) =>
          label.type === 'rectangle' ? (
            <LabelRect key={label.id} info={label} />
          ) : (
            <LabelPolygon key={label.id} info={label} />
          )
        )}
      </Layer>
    </Stage>
  );
}
```

**기능**:
- **사각형 도구**: 클릭 & 드래그로 바운딩 박스 그리기
- **폴리곤 도구**: 클릭하여 점 추가, 첫 번째 점 클릭하여 닫기
- **확대/축소**: Ctrl + 스크롤로 정밀 라벨링
- **팬**: 캔버스를 드래그하여 큰 이미지 탐색

### 4. YOLOv8 자동 라벨링 통합

충분한 수동 라벨이 있으면 (~100개 이미지), 모델 학습 트리거:

```python
from fastapi import FastAPI
from ultralytics import YOLO

app = FastAPI()
model = YOLO('best.pt')

@app.post("/predict")
async def predict(file: UploadFile):
    img = cv2.imdecode(np.frombuffer(contents, np.uint8), cv2.IMREAD_COLOR)
    results = model.predict(img, conf=0.5)

    predictions = []
    for result in results:
        for box, mask in zip(result.boxes, result.masks):
            predictions.append({
                "class": int(box.cls),
                "confidence": float(box.conf),
                "bbox": box.xyxy.tolist(),
                "mask": mask.xy.tolist()
            })

    return {"predictions": predictions}
```

## 성능 지표

### 업로드 최적화

```
이전 (전통적):
[클라이언트] ──[2GB]──> [서버] ──[2GB]──> [S3]
        총 18분

이후 (Presigned URL):
[클라이언트] ──[10KB 메타데이터]──> [서버]
[클라이언트] ──────[2GB]───────> [S3] (직접)
        총 90초
```

### 렌더링 최적화

```
이전 (전체 렌더):
- DOM 노드: 1000행
- 초기 렌더: 5-10초
- 스크롤 FPS: 15-20

이후 (가상화):
- DOM 노드: ~15개 표시 행
- 초기 렌더: <100ms
- 스크롤 FPS: 60
```

## 배운 점

### 1. 대용량 파일에는 Presigned URL > 서버 업로드

10MB 이상의 파일 업로드에는 presigned URL이 기본값이어야 합니다:
- S3의 인프라로 대역폭 오프로드
- 클라이언트에서 병렬 업로드 가능
- 서버 비용 감소 (EC2 데이터 전송 없음)

### 2. 긴 목록에는 react-window가 필수

50개 이상의 항목을 렌더링할 때마다:
- 가상화 사용 (react-window 또는 react-virtuoso)
- React.memo로 행 컴포넌트 메모이제이션
- `loading="lazy"`로 이미지 지연 로드

### 3. 간단한 상태에는 Zustand > Redux

Zustand 장점:
- **3KB** vs Redux의 12KB + 미들웨어
- 보일러플레이트 없음 (액션, 리듀서, 스토어 설정)
- 내장 TypeScript 지원

## 라이브 데모

소스 코드: [GitHub - worlabel](https://github.com/HyunjoJung/worlabel)

---

**질문이나 피드백이 있으신가요?** [GitHub](https://github.com/HyunjoJung)에서 연락해주세요
