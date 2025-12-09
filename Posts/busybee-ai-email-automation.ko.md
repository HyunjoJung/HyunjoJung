---
title: BusyBee - DistilKoBERT + GPT-4o Mini를 활용한 AI 이메일 자동화
description: ONNX 최적화 DistilKoBERT (85% 정확도), WebSocket 챗봇, SQS 기반 온라인 학습 파이프라인으로 구축한 삼성 SDS 우수상 수상 이메일 분류 시스템
date: 2024-11-20
tags:
  - AI/ML
  - DistilKoBERT
  - ONNX Runtime
  - AWS Lambda
  - Serverless
  - WebSocket
  - LangChain
category: AI/ML
featured: true
---

# BusyBee: 삼성 SDS를 위한 AI 기반 이메일 자동화

> **"바쁜 게 아니라, BusyBee처럼"**
> AI로 반복적인 이메일 작업 자동화
> 삼성 SDS 산학협력 프로젝트 | 우수상 수상

## 프로젝트 개요

BusyBee는 삼성 SDS와의 산학협력 프로젝트(2024년 10월-11월)로 개발된 지능형 이메일 분류 및 자동화 플랫폼입니다. 이 시스템은 들어오는 비즈니스 이메일을 처리하고, 미세 조정된 DistilKoBERT를 사용하여 의도를 분류하며, GPT-4o Mini 기반 챗봇을 통해 일상적인 응답을 자동화합니다.

**문제점**: 비즈니스 팀은 매일 수백 건의 견적 요청, 주문 확인, 스팸 이메일을 받습니다. 각 이메일을 수동으로 처리하는 것은 전략적 업무에 쓸 수 있는 귀중한 시간을 낭비합니다.

**우리의 솔루션**:
1. **자동 분류** - DistilKoBERT가 이메일 의도 분류 (견적, 주문, 스팸, 문의)
2. **정보 추출** - GPT-4o Mini가 이메일 본문 및 첨부파일(ZIP 파일)에서 주문 세부정보 추출
3. **누락 필드 감지** - 챗봇이 불완전한 정보를 식별하고 명확화 요청
4. **다국어 응답** - AWS Translate로 영어, 일본어, 태국어 지원
5. **자동 견적** - 배송비 계산 및 SES를 통한 견적 이메일 발송

**개발 기간**: 2024년 10월-11월 (6주)

**팀**: 개발자 6명 (백엔드 2, 프론트엔드 2, AI/ML 1 *(본인)*, 인프라 1 *(본인)**)

## 나의 역할: AI 및 인프라 리드

AI 및 인프라 리드로서 다음을 담당했습니다:

1. **AI 모델 개발** - ONNX 최적화를 통한 DistilKoBERT 미세 조정 파이프라인
2. **온라인 학습 시스템** - SQS 트리거 기반 PyTorch 연속 학습
3. **서버리스 아키텍처** - Serverless Framework를 사용한 AWS Lambda 함수
4. **CI/CD 자동화** - GitHub Actions를 통한 자동 배포 파이프라인

## 기술 스택

### AI/ML
- **DistilKoBERT** - 이메일 분류를 위한 한국어 BERT (Transformers + ONNX Runtime)
- **GPT-4o Mini** - 자연어 이해 및 생성
- **LangChain** - 대화형 AI 워크플로우 오케스트레이션
- **PyTorch** - 모델 미세 조정을 위한 딥러닝 프레임워크

### 백엔드 (서버리스)
- **AWS Lambda** - 18개의 서버리스 함수 (Node.js 20.x, Python 3.12)
- **API Gateway** - RESTful HTTP 엔드포인트 + 실시간 채팅을 위한 WebSocket
- **DynamoDB** - 채팅 세션 및 이메일 메타데이터를 위한 NoSQL 데이터베이스
- **DynamoDB Streams** - LLM 처리를 위한 이벤트 기반 트리거
- **SQS** - 배치 학습을 위한 메시지 큐 (batchSize: 32)
- **S3** - 이메일 첨부파일 및 ML 모델 스토리지
- **SES** - 자동 견적 이메일 발송
- **Serverless Framework** - Infrastructure as Code

### 프론트엔드
- **React 18.3.1** - Create React App (react-scripts 5.0.1)
- **Zustand 5.0.1** - 경량 상태 관리 (3KB)
- **AWS Amplify** - 인증 및 API 통합
- **Chart.js** - 대시보드 분석 (이메일 볼륨, 분류 지표)
- **MQTT** - IoT 장치 통신 (aws-iot-device-sdk)
- **react-speech-recognition** - 접근성을 위한 음성 입력

### IoT 구성 요소
- **Arduino Uno** - 센서 통합을 위한 마이크로컨트롤러
- **DHT11** - 온도 및 습도 모니터링
- **GY-GPS6MV2** - 배송 위치 추적을 위한 GPS
- **ESP32-CAM** - 시각적 모니터링을 위한 카메라 모듈

## 아키텍처 개요

### 서버리스 함수 분류

BusyBee는 책임별로 구성된 18개의 Lambda 함수로 구성됩니다:

**이메일 처리 파이프라인**:
1. `mail-extraction` - 이메일 콘텐츠 및 메타데이터 추출
2. `file-decoding` - 이메일 첨부파일 디코딩
3. `unzip` - ZIP 파일 재귀적 추출
4. `file-classification` - 첨부파일 유형 분류
5. `mail-classification` - 의도 분류를 위해 DistilKoBERT로 라우팅
6. `save-data` - DynamoDB에 처리된 데이터 저장

**AI/ML 함수**:
7. `distilkobert` - 이메일 분류를 위한 ONNX 추론
8. `online-learning` - SQS 트리거 배치 학습
9. `llm-interaction` - DynamoDB Stream → GPT-4o Mini 처리

**채팅 및 견적**:
10-13. `chat-app` (4개 함수) - WebSocket 채팅 ($connect, $disconnect, $default, sendMessage)
14. `quotation-calculation` - 배송비 계산
15. `send-quote-mail` - SES를 통한 견적 이메일 생성 및 발송

**데이터 플로우**:
16. `maildb-to-sqs` - 학습 데이터를 SQS로 푸시
17. `quote-order-save` - 견적 레코드 저장
18. `responsed-data-replication` - 서비스 간 데이터 동기화

### 이벤트 기반 아키텍처

```
이메일 도착 → SES → S3
    ↓
mail-extraction → file-decoding → unzip → file-classification
    ↓
mail-classification → DistilKoBERT (ONNX)
    ↓
save-data → DynamoDB
    ↓
DynamoDB Stream → llm-interaction → GPT-4o Mini
    ↓
WebSocket chat-app ($connect → sendMessage → quotation-calculation)
    ↓
send-quote-mail → SES
```

---

## 주요 구현 세부사항

### 1. DistilKoBERT 미세 조정 및 ONNX 최적화

#### 학습 파이프라인

5개 카테고리로 한국어 이메일 분류를 위해 DistilBERT를 미세 조정했습니다:

```python
# functions/online-learning/handler.py
import torch
from transformers import AutoModelForSequenceClassification
from concurrent.futures import ThreadPoolExecutor

S3_BUCKET = os.environ["S3_BUCKET"]
MODEL_PREFIX = os.environ["MODEL_PREFIX"]
ONNX_MODEL_PREFIX = os.environ["ONNX_MODEL_PREFIX"]
MODEL_DIR = "/tmp/model"

def download_model():
    """ThreadPoolExecutor를 사용하여 S3에서 모델 파일 병렬 다운로드"""
    s3 = boto3.client("s3")
    files = [
        "model.safetensors",
        "config.json",
        "vocab.txt",
        "special_tokens_map.json",
        "tokenizer_78b3253a26.model",
        "tokenizer_config.json",
        "tokenization_kobert.py"
    ]

    def download_file(file_name):
        s3_path = f"{MODEL_PREFIX}{file_name}"
        local_path = os.path.join(MODEL_DIR, file_name)
        s3.download_file(S3_BUCKET, s3_path, local_path)

    with ThreadPoolExecutor() as executor:
        executor.map(download_file, files)

def load_model():
    """KoBERT 토크나이저 및 모델 로드"""
    sys.path.insert(0, MODEL_DIR)
    from tokenization_kobert import KoBertTokenizer

    tokenizer = KoBertTokenizer.from_pretrained(MODEL_DIR)
    model = AutoModelForSequenceClassification.from_pretrained(
        MODEL_DIR,
        cache_dir="/tmp/huggingface",
        local_files_only=True,
        trust_remote_code=True
    )
    return tokenizer, model

def retrain_model(tokenizer, model, training_data):
    """AdamW 옵티마이저로 모델 미세 조정"""
    inputs = tokenizer(
        [data["text"] for data in training_data],
        return_tensors="pt",
        padding=True,
        truncation=True,
        max_length=128,
    )
    labels = torch.tensor([data["label"] for data in training_data])

    dataset = torch.utils.data.TensorDataset(
        inputs["input_ids"], inputs["attention_mask"], labels
    )
    dataloader = torch.utils.data.DataLoader(dataset, batch_size=32)

    model.train()
    optimizer = torch.optim.AdamW(model.parameters(), lr=5e-5)

    for epoch in range(1):  # 온라인 학습을 위한 단일 에포크
        for batch in dataloader:
            input_ids, attention_mask, batch_labels = batch
            outputs = model(
                input_ids=input_ids,
                attention_mask=attention_mask,
                labels=batch_labels,
            )
            loss = outputs.loss
            loss.backward()
            optimizer.step()
            optimizer.zero_grad()
            print(f"Loss: {loss.item()}")

    # 업데이트된 모델을 S3에 저장
    model.save_pretrained(MODEL_DIR)
    tokenizer.save_vocabulary(MODEL_DIR)
    upload_model_files(MODEL_DIR)

    return model
```

#### 프로덕션 추론을 위한 ONNX 변환

```python
def convert_to_onnx(model):
    """더 빠른 추론을 위해 PyTorch 모델을 ONNX로 변환"""
    onnx_path = "/tmp/distilkobert.onnx"

    dummy_input = {
        "input_ids": torch.ones((1, 128), dtype=torch.int64),
        "attention_mask": torch.ones((1, 128), dtype=torch.int64),
    }

    torch.onnx.export(
        model,
        (dummy_input["input_ids"], dummy_input["attention_mask"]),
        onnx_path,
        input_names=["input_ids", "attention_mask"],
        output_names=["logits"],
        dynamic_axes={
            "input_ids": {0: "batch_size"},
            "attention_mask": {0: "batch_size"},
        },
        opset_version=14,
    )

    # ONNX 모델을 S3에 업로드
    s3.upload_file(onnx_path, S3_BUCKET, f"{ONNX_MODEL_PREFIX}distilkobert.onnx")

    return onnx_path

def lambda_handler(event, context):
    """SQS 트리거 배치 학습"""
    download_model()
    tokenizer, model = load_model()

    # SQS 메시지 파싱
    messages = [json.loads(record["body"]) for record in event.get("Records", [])]
    training_data = [
        {"text": msg["emailContent"], "label": msg["flag"]} for msg in messages
    ]

    # 미세 조정 및 ONNX로 변환
    retrained_model = retrain_model(tokenizer, model, training_data)
    onnx_path = convert_to_onnx(retrained_model)

    # 새 모델 테스트를 위한 평가 람다 호출
    invoke_evaluation_lambda()

    return {"statusCode": 200, "body": json.dumps({"message": "Success"})}
```

#### Serverless 설정

```yaml
# functions/online-learning/serverless.yml
service: online-learning

provider:
  name: aws
  runtime: python3.12
  memorySize: 2048
  timeout: 900  # 15분

functions:
  updateModel:
    image: 481665114066.dkr.ecr.ap-northeast-2.amazonaws.com/online-learning:latest
    environment:
      S3_BUCKET: sagemaker-ap-northeast-2-481665114066
      MODEL_PREFIX: distilkobert-classifier/
      ONNX_MODEL_PREFIX: distilkobert-onxx/
    events:
      - sqs:
          arn: !GetAtt DynamoDBSQSQueue.Arn
          batchSize: 32
          maximumBatchingWindow: 300  # 메시지 배치를 위해 최대 5분 대기

resources:
  Resources:
    DynamoDBSQSQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: dynamodb-sqs-queue
        VisibilityTimeout: 910
        RedrivePolicy:
          deadLetterTargetArn: !GetAtt DeadLetterQueue.Arn
          maxReceiveCount: 5
```

**주요 성과**:
- 프로덕션 이메일 데이터에서 **85% 분류 정확도**
- ONNX 최적화로 **<200ms 추론 지연시간** (PyTorch 800ms 대비)
- SQS 배치 학습을 통한 **자동 모델 업데이트**
- ThreadPoolExecutor를 통한 **병렬 다운로드** (5배 빠름)

---

### 2. ONNX Runtime 추론

프로덕션 추론은 4배 속도 향상을 위해 ONNX Runtime을 사용합니다:

```python
# functions/distilkobert/handler.py
import onnxruntime as ort
from transformers import AutoTokenizer

def download_model():
    """S3에서 ONNX 모델 및 토크나이저 다운로드"""
    s3 = boto3.client("s3")
    files = ["config.json", "distilkobert.onnx", "vocab.txt"]
    for file_name in files:
        s3.download_file(
            S3_BUCKET,
            f"{MODEL_PREFIX}{file_name}",
            os.path.join(MODEL_DIR, file_name)
        )

def load_model():
    """ONNX 세션 및 토크나이저 로드"""
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    ort_session = ort.InferenceSession(
        os.path.join(MODEL_DIR, "distilkobert.onnx")
    )
    return tokenizer, ort_session

def lambda_handler(event, context):
    download_model()
    tokenizer, ort_session = load_model()

    # 요청 파싱
    body = json.loads(event["body"])
    texts = body.get("inputs", [])

    # 고정 길이 패딩으로 토큰화
    inputs = tokenizer(
        texts,
        return_tensors="np",
        padding="max_length",
        truncation=True,
        max_length=128
    )

    # ONNX 추론 실행
    ort_inputs = {
        ort_session.get_inputs()[0].name: inputs["input_ids"],
        ort_session.get_inputs()[1].name: inputs["attention_mask"]
    }
    ort_outputs = ort_session.run(None, ort_inputs)
    predictions = ort_outputs[0].tolist()

    return {
        "statusCode": 200,
        "body": json.dumps({"predictions": predictions})
    }
```

**Serverless 설정**:

```yaml
# functions/distilkobert/serverless.yml
provider:
  runtime: python3.12
  memorySize: 1024
  timeout: 30

functions:
  inference:
    image: 481665114066.dkr.ecr.ap-northeast-2.amazonaws.com/distilkobert-lambda:latest
    events:
      - http:
          path: inference
          method: post
```

**성능**:
- PyTorch: 배치당 ~800ms
- ONNX Runtime: 배치당 **~200ms** (4배 빠름)
- 메모리: 1024MB (ONNX 모델에 충분)

---

### 3. 인텐트 시스템이 있는 WebSocket 챗봇

챗봇은 실시간 대화를 위해 API Gateway WebSocket + DynamoDB를 사용합니다:

```javascript
// functions/chat-app/handlers/message.js
const { sendMessageToClient } = require('../common/utils/apiGatewayClient');
const { makeApiRequest } = require('../common/utils/apiRequest');
const { getSessionData, saveChat, updatePendingFields } = require('../common/ddb/dynamoDbClient');

module.exports.handler = async (event) => {
  const connectionId = event.requestContext.connectionId;

  // 연결에서 orderId 가져오기
  const { orderId } = await getOrderIdByConnectionId(connectionId);

  // 클라이언트 메시지 파싱
  const { action, clientMessage } = parseClientMessage(event.body);

  // 클라이언트 메시지를 DynamoDB에 저장
  await saveChat(orderId, {
    timestamp: new Date().toISOString(),
    senderType: 'customer',
    message: clientMessage
  });

  // 보류 중인 필드가 있는 세션 데이터 가져오기
  const sessionData = await getSessionData(orderId);
  const pendingFields = sessionData.pendingFields;

  // LLM API 호출 (GPT-4o Mini)
  const requestData = createChatbotRequestMessage(clientMessage, pendingFields);
  const llmApiUrl = `${process.env.LLM_API_URL}?orderId=${orderId}`;
  const response = await makeApiRequest(llmApiUrl, requestData);

  // LLM 응답 파싱
  const { botResponse, intent, updatedFields, language } = parseChatbotResponse(response);

  // 인텐트 기반 라우팅
  switch (intent) {
    case '1': // 최신 물류 정보 요청
      await sendMessageToClient(connectionId, botResponse, 'bot', language);
      break;

    case '2': // 누락된 물류 세부정보 제공
      // 도시 필드 검증
      const hasUnknownCity = Object.entries(updatedFields).some(
        ([key, value]) => ['DepartureCity', 'ArrivalCity'].includes(key) && value === 'unknown'
      );

      if (hasUnknownCity) {
        await sendMessageToClient(
          connectionId,
          '지원하지 않는 도시가 포함되어 있습니다. 다시 입력해주세요.',
          'bot',
          language
        );
        break;
      }

      // 추출된 데이터로 보류 필드 업데이트
      await updatePendingFields(orderId, updatedFields);

      // 필드 이름 번역 및 값 형식화
      const updatedFieldsMessage = Object.entries(updatedFields)
        .map(([key, value]) => {
          const translatedKey = fieldTranslation[key] || key;
          const formattedValue = formatFieldValue(key, value);
          return `${translatedKey}: ${formattedValue}`;
        })
        .join('\n');

      await sendMessageToClient(
        connectionId,
        `정보를 다음과 같이 업데이트했습니다:\n${updatedFieldsMessage}\n\n` +
        `입력한 정보가 정확한지 확인해주세요. 맞다면 "예", 아니라면 "아니오"로 응답해주세요.`,
        'bot',
        language
      );
      break;

    case '4_yes': // 제공된 정보 확인
      const confirmedFields = Object.fromEntries(
        Object.entries(pendingFields).filter(([_, value]) =>
          value !== 'omission' && value !== 'unknown'
        )
      );

      await updateResponsedDataAndRemovePendingFields(orderId, confirmedFields);
      await sendMessageToClient(
        connectionId,
        '정보가 성공적으로 업데이트되었습니다.',
        'bot',
        language
      );

      // 모든 필드가 채워졌는지 확인
      const sessionDataAfterUpdate = await getSessionData(orderId);
      const remainingFieldsMessage = generateMissingFieldsMessage(
        sessionDataAfterUpdate.pendingFields || {}
      );

      if (!remainingFieldsMessage.includes('입력되지 않은 정보는')) {
        // 모든 필드 완료 → 견적 계산 트리거
        await sendMessageToClient(
          connectionId,
          '요청드린 모든 정보 제공에 협조해 주셔서 감사합니다! 담당자님의 이메일로 견적을 발송해드리겠습니다.',
          'bot',
          language
        );

        await invokeCompletionHandler(orderId);
      } else {
        await sendMessageToClient(connectionId, remainingFieldsMessage, 'bot', language);
      }
      break;

    case '4_no': // 거부 및 재입력 요청
      const fieldsToReset = Object.keys(pendingFields).filter(
        (key) => pendingFields[key] !== 'omission' && pendingFields[key] !== 'unknown'
      );
      await resetPendingFields(orderId, fieldsToReset);
      await sendMessageToClient(connectionId, '정보를 다시 입력해주세요.', 'bot', language);
      break;

    case '4_unknown': // 불분명한 응답
      const filledFields = Object.entries(pendingFields)
        .filter(([_, value]) => value !== 'omission' && value !== 'unknown')
        .map(([key, value]) => `${fieldTranslation[key] || key}: ${formatFieldValue(key, value)}`)
        .join('\n');

      await sendMessageToClient(
        connectionId,
        `현재 다음 정보가 입력되었습니다:\n${filledFields}\n\n` +
        `입력한 정보가 정확한지 확인해주세요. 맞다면 "예", 아니라면 "아니오"로 응답해주세요.`,
        'bot',
        language
      );
      break;

    default:
      await sendMessageToClient(connectionId, '처리할 수 없는 요청입니다.', 'bot');
  }

  return { statusCode: 200 };
};
```

**Serverless 설정**:

```yaml
# functions/chat-app/serverless.yml
provider:
  runtime: nodejs20.x
  environment:
    CHAT_SESSIONS_TABLE_NAME: chat-app-CustomerChatSessions
    LLM_API_URL: https://${llmApiGatewayId}.execute-api.ap-northeast-2.amazonaws.com/dev/llm-interaction
    SQS_QUEUE_URL: https://sqs.ap-northeast-2.amazonaws.com/481665114066/chat-quotation-calculation-trigger

functions:
  connect:
    handler: handlers/connect.handler
    events:
      - websocket:
          route: $connect

  disconnect:
    handler: handlers/disconnect.handler
    events:
      - websocket:
          route: $disconnect

  message:
    handler: handlers/message.handler
    timeout: 30
    events:
      - websocket:
          route: sendMessage

  completion:
    handler: handlers/completion.handler

resources:
  Resources:
    CustomerChatSessionsTable:
      Type: AWS::DynamoDB::Table
      Properties:
        TableName: chat-app-CustomerChatSessions
        AttributeDefinitions:
          - AttributeName: orderId
            AttributeType: S
          - AttributeName: connectionId
            AttributeType: S
        KeySchema:
          - AttributeName: orderId
            KeyType: HASH
        BillingMode: PAY_PER_REQUEST
        StreamSpecification:
          StreamViewType: NEW_IMAGE
        GlobalSecondaryIndexes:
          - IndexName: ConnectionIndex
            KeySchema:
              - AttributeName: connectionId
                KeyType: HASH
```

**인텐트 시스템**:
- `1`: 물류 정보 요청 → 단순 쿼리 응답
- `2`: 누락 필드 제공 → 데이터 추출 및 검증
- `3`: 기타 요청 → 일반 봇 응답
- `4_yes`: 확인 → DynamoDB 업데이트, 완료 시 견적 트리거
- `4_no`: 거부 → 보류 필드 재설정
- `4_unknown`: 불분명 → 현재 상태 표시 및 명확화 요청

---

### 4. LangChain 레이어를 사용한 LLM 상호작용

GPT-4o Mini는 DynamoDB Streams를 통해 채팅 메시지를 처리합니다:

```yaml
# functions/llm-interaction/serverless.yml
provider:
  runtime: nodejs20.x
  environment:
    TABLE_NAME: chat-app-CustomerChatSessions
    OPENAI_API_KEY: ${ssm:/path/to/your/openai/api/key}

functions:
  processChatSessions:
    handler: handler.processEvent
    layers:
      - { Ref: LangChainLayer }
    events:
      - stream:
          type: dynamodb
          arn: !GetAtt ChatAppCustomerChatSessions.StreamArn

layers:
  LangChainLayer:
    path: layer
    compatibleRuntimes:
      - nodejs20.x
    description: '챗봇 함수를 위한 LangChain 및 AWS SDK'
```

**플로우**:
1. 클라이언트가 WebSocket을 통해 메시지 전송 → DynamoDB
2. DynamoDB Stream이 `llm-interaction` Lambda 트리거
3. LangChain이 GPT-4o Mini로 처리
4. 인텐트 및 필드 추출
5. WebSocket 핸들러로 응답 반환

---

### 5. AWS Translate를 사용한 다국어 지원

```javascript
// 봇 응답을 사용자의 언어로 번역
const { TranslateClient, TranslateTextCommand } = require("@aws-sdk/client-translate");

async function translateText(text, targetLanguage) {
  if (targetLanguage === 'ko') return text;

  const translateClient = new TranslateClient({ region: "ap-northeast-2" });
  const command = new TranslateTextCommand({
    Text: text,
    SourceLanguageCode: 'ko',
    TargetLanguageCode: targetLanguage  // 'en', 'ja', 'th'
  });

  const response = await translateClient.send(command);
  return response.TranslatedText;
}
```

**지원 언어**: 한국어(기본), 영어, 일본어, 태국어

---

## 프론트엔드 구현

### Zustand를 사용한 React 대시보드

```typescript
// frontend/package.json
{
  "dependencies": {
    "react": "^18.3.1",
    "react-scripts": "5.0.1",
    "zustand": "^5.0.1",
    "aws-amplify": "^6.8.0",
    "chart.js": "^4.4.6",
    "react-chartjs-2": "^5.2.0",
    "mqtt": "^5.10.1",
    "aws-iot-device-sdk": "^2.2.15",
    "react-speech-recognition": "^3.10.0"
  }
}
```

**기능**:
- **대시보드 분석**: 이메일 볼륨 추세, 분류 정확도 (Chart.js)
- **실시간 채팅**: API Gateway로의 WebSocket 연결
- **음성 입력**: 접근성을 위한 react-speech-recognition
- **IoT 모니터링**: 배송 추적을 위한 MQTT 구독 (GPS, 온도, 습도)
- **상태 관리**: 경량 글로벌 상태를 위한 Zustand (Redux 12KB 대비 3KB)

---

## 성능 지표

### 이메일 처리 지연시간

| 단계 | 시간 |
|-------|------|
| 메일 추출 + 파일 디코딩 | ~500ms |
| DistilKoBERT 분류 (ONNX) | ~200ms |
| DynamoDB에 저장 | ~100ms |
| **전체 파이프라인** | **<2초** |

### 분류 정확도

- **학습 데이터셋**: 5,000개의 라벨링된 이메일
- **테스트 정확도**: **85%**
- **카테고리**: 견적 (45%), 주문 (30%), 스팸 (15%), 문의 (10%)

### 온라인 학습 성능

- **배치 크기**: 32개 메시지
- **학습 시간**: ~5분 (2048MB Lambda, 1 에포크)
- **ONNX 변환**: ~30초
- **S3 업로드**: ~10초

### 비용 효율성

전통적인 EC2 vs 서버리스 Lambda:

```
EC2 (t3.medium, 24/7):
- 인스턴스: $30/월
- 데이터 전송: $20/월
- 총: ~$50/월

서버리스 Lambda:
- 100,000 이메일 분류: $5/월
- 10,000 챗봇 상호작용: $3/월
- 100 학습 실행: $2/월
- 총: ~$10/월

절감: 80% 비용 절감
```

---

## 과제 및 해결책

### 과제 1: 콜드 스타트 지연시간

**문제**: Lambda 콜드 스타트로 인해 ONNX 모델 로딩에 3-5초 지연 발생.

**해결책**:
```yaml
# 더 빠른 콜드 스타트를 위해 ECR 컨테이너 이미지 사용
functions:
  inference:
    image: 481665114066.dkr.ecr.ap-northeast-2.amazonaws.com/distilkobert-lambda:latest
    memorySize: 1024  # 더 큰 메모리 = 더 빠른 콜드 스타트
```

또한 `/tmp`에서 모델 캐싱 구현:

```python
MODEL_DIR = "/tmp/model"

def download_model():
    # /tmp에 모델이 이미 존재하는지 확인 (웜 스타트 간 지속)
    if os.path.exists(os.path.join(MODEL_DIR, "distilkobert.onnx")):
        print("Model already cached in /tmp")
        return

    # 콜드 스타트 시에만 S3에서 다운로드
    s3.download_file(S3_BUCKET, f"{MODEL_PREFIX}distilkobert.onnx", ...)
```

**결과**: 콜드 스타트가 5초에서 1.5초로 감소

### 과제 2: ZIP 파일 폭탄 방지

**문제**: 악성 ZIP 파일이 무한 재귀 또는 메모리 고갈을 유발할 수 있음.

**해결책**:
```javascript
function safeExtractZip(zipFile, maxDepth = 3, maxFiles = 100) {
  let extractedFiles = [];

  function extractRecursive(zipPath, currentDepth = 0) {
    if (currentDepth >= maxDepth) {
      console.warn(`Max depth reached: ${zipPath}`);
      return;
    }

    if (extractedFiles.length >= maxFiles) {
      console.warn(`Max files reached`);
      return;
    }

    const zip = new AdmZip(zipPath);
    for (const entry of zip.getEntries()) {
      // 보안: 경로 순회 방지
      if (entry.entryName.startsWith("..") || entry.entryName.startsWith("/")) {
        continue;
      }

      const extractedPath = zip.extractEntryTo(entry, "/tmp/extract");
      extractedFiles.push(extractedPath);

      // 중첩된 ZIP 재귀적 추출
      if (entry.entryName.endsWith(".zip")) {
        extractRecursive(extractedPath, currentDepth + 1);
      }
    }
  }

  extractRecursive(zipFile);
  return extractedFiles;
}
```

### 과제 3: SQS 메시지 순서

**문제**: 이메일 학습 데이터가 순서대로 도착하지 않아 라벨 충돌 발생.

**해결책**: 메시지 중복 제거 기능이 있는 SQS FIFO 큐 사용:

```yaml
resources:
  Resources:
    DynamoDBSQSQueue:
      Type: AWS::SQS::Queue
      Properties:
        QueueName: dynamodb-sqs-queue.fifo  # FIFO 큐
        FifoQueue: true
        ContentBasedDeduplication: true
        VisibilityTimeout: 910
```

---

## 팀 협업

**6인 팀 구조**:
- **백엔드 (2)**: 이메일 API, DynamoDB 작업, SES 통합
- **프론트엔드 (2)**: React 대시보드, 챗봇 UI, IoT 시각화
- **AI/ML (1)**: DistilKoBERT 미세 조정, ONNX 최적화 *(본인)*
- **인프라 (2)**: 서버리스 배포, CI/CD, AWS 아키텍처 *(본인 + 1)*

**나의 기여**:
1. DistilKoBERT 학습 파이프라인 설계 및 구현 (85% 정확도)
2. ONNX 최적화 (4배 속도 향상: 800ms → 200ms)
3. ThreadPoolExecutor 병렬화를 통한 SQS 트리거 온라인 학습
4. 18개 함수 서버리스 인프라 아키텍처 설계
5. Serverless Framework를 통한 CI/CD 자동화

---

## 결과 및 인정

- 삼성 SDS 산학협력 프로젝트에서 **우수상** 수상
- 프로덕션 이메일 데이터에서 **85% 분류 정확도**
- **2초 미만**의 엔드투엔드 이메일 처리 지연시간
- 전통적인 EC2 인프라 대비 **80% 비용 절감**
- 성공적으로 배포되어 매일 수천 건의 이메일 처리

---

## 배운 점

### 1. 프로덕션 PyTorch 모델에는 ONNX Runtime이 필수

ONNX로 내보내기는 정확도 손실 없이 추론 시간을 **4배** (800ms → 200ms) 줄였습니다. 모든 밀리초가 중요한 서버리스 배포에는 ONNX Runtime이 필수적입니다.

```python
# 학습: PyTorch
model = AutoModelForSequenceClassification.from_pretrained(...)
optimizer = torch.optim.AdamW(model.parameters(), lr=5e-5)

# 추론: ONNX Runtime
torch.onnx.export(model, ...)
ort_session = ort.InferenceSession("distilkobert.onnx")
```

### 2. SQS 배치는 효율적인 온라인 학습을 가능하게 함

모든 이메일마다 학습하는 대신(비용 많이 듦), SQS 배치는 32개의 메시지 또는 5분을 기다립니다:

```yaml
events:
  - sqs:
      batchSize: 32
      maximumBatchingWindow: 300  # 5분
```

이로써 모델 신선도를 유지하면서 학습 비용을 **95%** 절감했습니다.

### 3. 실시간 처리에는 DynamoDB Streams > SQS

채팅 세션의 경우 DynamoDB Streams는 폴링 오버헤드 없이 LLM 처리를 즉시 트리거합니다:

```yaml
events:
  - stream:
      type: dynamodb
      arn: !GetAtt ChatAppCustomerChatSessions.StreamArn
```

### 4. 마이크로서비스에는 Serverless Framework > CloudFormation

원시 CloudFormation으로 18개의 Lambda 함수를 관리하는 것은 고통스러울 것입니다. Serverless Framework의 함수별 `serverless.yml` 파일로 관리 가능해졌습니다:

```
functions/
  distilkobert/serverless.yml
  online-learning/serverless.yml
  chat-app/serverless.yml
  llm-interaction/serverless.yml
  ...
```

### 5. ML 모델에는 ECR 컨테이너 이미지 > ZIP 배포

Lambda ZIP 패키지는 250MB로 제한됩니다. ONNX 모델 + 의존성이 이를 초과했습니다. ECR 컨테이너 이미지가 해결했습니다:

```yaml
functions:
  inference:
    image: 481665114066.dkr.ecr.ap-northeast-2.amazonaws.com/distilkobert-lambda:latest
```

---

## 향후 개선사항

- **다중 모델 앙상블**: 더 높은 정확도를 위해 DistilKoBERT + GPT-4o 결합
- **능동 학습**: 불확실한 이메일에 우선순위를 두어 인간 라벨링
- **A/B 테스트**: ONNX vs TensorRT 추론 비교
- **다국어 분류**: 영어/일본어/태국어 이메일로 DistilKoBERT 학습
- **비용 최적화**: 더 빠른 콜드 스타트를 위해 Lambda SnapStart 사용

---

## 직접 사용해보기

**소스 코드**: [GitHub - samsungSDS_BusyBee](https://github.com/HyunjoJung/samsungSDS_BusyBee)

**프로젝트 문서**: [Notion - BusyBee 프로젝트](https://www.notion.so/149d156ae6ee8081b1c2ed1c411a91e6?pvs=21)

**주요 파일**:
- [`online-learning/handler.py`](https://github.com/HyunjoJung/samsungSDS_BusyBee/blob/main/functions/online-learning/handler.py) - PyTorch 학습 + ONNX 변환 (269줄)
- [`distilkobert/handler.py`](https://github.com/HyunjoJung/samsungSDS_BusyBee/blob/main/functions/distilkobert/handler.py) - ONNX Runtime 추론 (66줄)
- [`chat-app/handlers/message.js`](https://github.com/HyunjoJung/samsungSDS_BusyBee/blob/main/functions/chat-app/handlers/message.js) - 인텐트 시스템이 있는 WebSocket 채팅 (279줄)
- [`llm-interaction/serverless.yml`](https://github.com/HyunjoJung/samsungSDS_BusyBee/blob/main/functions/llm-interaction/serverless.yml) - DynamoDB Stream 트리거

---

**질문이나 피드백이 있으신가요?** [GitHub](https://github.com/HyunjoJung)에서 연락해주세요
