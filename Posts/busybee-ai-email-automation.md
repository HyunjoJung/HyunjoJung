---
title: BusyBee - AI-Powered Email Automation for Samsung SDS
description: Building an intelligent email classification and automation platform with GPT-4, LangChain, and serverless architecture - winning Samsung SDS excellence award.
date: 2024-11-20
tags:
  - AI/ML
  - LangChain
  - AWS Lambda
  - Serverless
  - IoT
  - GPT-4
category: AI/ML
featured: true
---

# BusyBee: AI-Powered Email Automation for Samsung SDS

> "Stop being busy, start being BusyBee - Automate repetitive email tasks with AI"
> Samsung SDS Corporate Partnership Project | Excellence Award Winner

## Project Overview

BusyBee is an AI-driven email classification and automation platform developed as a corporate partnership project with Samsung SDS (October-November 2024). The system intelligently processes incoming emails, classifies their intent, and automates routine business responses - combining LLM technology with IoT monitoring capabilities.

### The Problem

Business teams receive hundreds of emails daily - quote requests, order confirmations, spam messages. Manually processing each email wastes valuable time that could be spent on strategic work. We needed a solution that could:

- **Automatically classify** email intent (quotes, orders, spam)
- **Extract information** from attachments including ZIP files
- **Identify missing data** and request clarification
- **Generate responses** in multiple languages
- **Calculate and send** automated quotations

## My Role: AI & Infrastructure Lead

As the AI and Infrastructure lead, I was responsible for:

### 1. AI Model Development

**DistilKoBERT Fine-tuning Pipeline**
```python
# SQS-triggered training pipeline
class EmailClassifier:
    def __init__(self):
        self.model = DistilBertForSequenceClassification.from_pretrained(
            'distilbert-base-multilingual-cased',
            num_labels=5  # Quote, Order, Spam, Inquiry, Other
        )

    async def train_on_new_data(self, sqs_event):
        # Triggered by SQS when new labeled data arrives
        training_data = self.load_from_dynamodb(sqs_event)

        # Fine-tune model
        trainer = Trainer(
            model=self.model,
            train_dataset=training_data,
            compute_metrics=self.compute_metrics
        )

        trainer.train()

        # Achieved 85% accuracy after iterative training
```

**Key achievements**:
- Built SQS-triggered training pipeline for continuous learning
- Achieved **85% classification accuracy** on production data
- Optimized inference latency to <200ms per email

### 2. LangChain Chatbot Architecture

**GPT-4o Mini + LangGraph for Information Extraction**
```python
from langchain.chains import LLMChain
from langchain.chat_models import ChatOpenAI
from langgraph.graph import StateGraph

class QuoteAssistant:
    def __init__(self):
        self.llm = ChatOpenAI(model="gpt-4o-mini", temperature=0)
        self.graph = self.build_workflow()

    def build_workflow(self):
        workflow = StateGraph()

        # Define conversation flow
        workflow.add_node("extract_info", self.extract_quote_details)
        workflow.add_node("identify_missing", self.find_missing_fields)
        workflow.add_node("generate_reply", self.create_multilingual_response)

        workflow.set_entry_point("extract_info")
        workflow.add_edge("extract_info", "identify_missing")
        workflow.add_conditional_edges(
            "identify_missing",
            self.check_completeness,
            {
                "complete": "generate_reply",
                "incomplete": "generate_reply"  # Ask for missing info
            }
        )

        return workflow.compile()

    async def process_email(self, email_content):
        # Extracts: quantity, product_code, delivery_date, etc.
        result = await self.graph.ainvoke({"email": email_content})

        # Automatically updates DynamoDB with extracted data
        await self.save_to_dynamodb(result)

        return result
```

**Capabilities**:
- Multilingual support (English, Japanese, Thai)
- Context-aware conversation flow
- Automatic DynamoDB updates with extracted information

### 3. Serverless Architecture Design

**Event-Driven Microservices**
```yaml
# serverless.yml
service: busybee-email-processor

functions:
  emailClassifier:
    handler: classifier.handler
    events:
      - sqs:
          arn: !GetAtt EmailQueue.Arn
          batchSize: 10
    environment:
      MODEL_BUCKET: ${self:custom.modelBucket}
      DYNAMODB_TABLE: ${self:custom.emailTable}

  chatbotProcessor:
    handler: chatbot.handler
    events:
      - apiGateway:
          path: /chat
          method: POST
    timeout: 30  # GPT-4 calls need extra time

  quoteGenerator:
    handler: quote.handler
    events:
      - eventBridge:
          pattern:
            source: ["busybee.email"]
            detail-type: ["QuoteRequest"]

resources:
  Resources:
    EmailQueue:
      Type: AWS::SQS::Queue
      Properties:
        VisibilityTimeout: 300

    EmailTable:
      Type: AWS::DynamoDB::Table
      Properties:
        BillingMode: PAY_PER_REQUEST
        AttributeDefinitions:
          - AttributeName: emailId
            AttributeType: S
        KeySchema:
          - AttributeName: emailId
            KeyType: HASH
```

**Architecture highlights**:
- **Cost-optimized**: Pay-per-use serverless functions
- **Auto-scaling**: Lambda handles traffic spikes automatically
- **Event-driven**: SQS, SNS, EventBridge for decoupling
- **Stateless design**: All state in DynamoDB

### 4. CI/CD Automation

**Serverless Framework + Jenkins Pipeline**
```groovy
pipeline {
    agent any

    stages {
        stage('Install Dependencies') {
            steps {
                sh 'npm install -g serverless'
                sh 'pip install -r requirements.txt'
            }
        }

        stage('Run Tests') {
            steps {
                sh 'pytest tests/ --cov=handlers'
            }
        }

        stage('Deploy to Staging') {
            steps {
                sh 'serverless deploy --stage staging'
            }
        }

        stage('Integration Tests') {
            steps {
                sh 'python integration_tests/test_email_flow.py'
            }
        }

        stage('Deploy to Production') {
            when {
                branch 'main'
            }
            steps {
                sh 'serverless deploy --stage prod'
            }
        }
    }

    post {
        always {
            junit 'test-results/*.xml'
        }
        success {
            slackSend(
                color: 'good',
                message: "BusyBee deployed successfully"
            )
        }
    }
}
```

## Tech Stack

### AI/ML
- **GPT-4o Mini**: Natural language understanding and generation
- **LangChain & LangGraph**: Conversational AI workflow orchestration
- **DistilKoBERT**: Lightweight Korean BERT for email classification
- **PyTorch**: Deep learning framework for model training

### Infrastructure
- **AWS Lambda**: Serverless compute for email processing
- **API Gateway**: RESTful API for chatbot and webhooks
- **DynamoDB**: NoSQL database for email metadata and conversation state
- **SES**: Email sending service for automated responses
- **S3**: Storage for email attachments and ML models
- **SQS & SNS**: Message queuing and pub/sub
- **Terraform**: Infrastructure as Code for reproducible deployments

### IoT Components
- **Arduino Uno**: Microcontroller for sensor integration
- **DHT11**: Temperature and humidity monitoring
- **GY-GPS6MV2**: GPS tracking for shipment location
- **ESP32-CAM**: Camera module for visual monitoring

### Frontend
- **React**: Interactive dashboard for email monitoring and chatbot interface

## Key Features

### 1. Intelligent Email Classification

The system analyzes both email content and attachments:

```python
async def classify_email(email):
    # Extract text from email body
    text_content = email.body

    # Process attachments (including ZIP files)
    attachments = await extract_attachments(email.files)

    # Combine text from all sources
    full_context = f"{text_content}\n\nAttachments:\n{attachments}"

    # DistilKoBERT classification
    category = await model.predict(full_context)

    # Route to appropriate handler
    if category == "QUOTE":
        await trigger_quote_workflow(email)
    elif category == "ORDER":
        await process_order(email)
    elif category == "SPAM":
        await mark_as_spam(email)
```

### 2. Automated Quote Generation

```python
class QuoteCalculator:
    def calculate_quote(self, product_code, quantity, delivery_location):
        # Lookup product pricing from DynamoDB
        product = self.get_product(product_code)

        # Calculate base price
        base_price = product.unit_price * quantity

        # Apply volume discount
        if quantity >= 1000:
            discount = 0.15
        elif quantity >= 500:
            discount = 0.10
        else:
            discount = 0.05

        # Calculate shipping cost
        shipping = self.calculate_shipping(delivery_location, quantity)

        total = base_price * (1 - discount) + shipping

        return {
            "base_price": base_price,
            "discount": discount,
            "shipping": shipping,
            "total": total,
            "valid_until": datetime.now() + timedelta(days=7)
        }

    async def send_quote_email(self, recipient, quote):
        # Generate professional quote email with SES
        template = self.render_quote_template(quote)

        await ses.send_email(
            to=recipient,
            subject="Your Quote Request",
            body=template
        )
```

### 3. Real-time IoT Dashboard

Shipment tracking dashboard showing:
- **GPS location** of containers
- **Temperature & humidity** readings
- **Camera feed** from ESP32-CAM
- **Alert notifications** for out-of-range conditions

## Performance Metrics

- **Email processing latency**: <2 seconds (including classification + chatbot)
- **Classification accuracy**: 85%
- **Automated quote generation**: 90% success rate (10% require human review)
- **Cost savings**: $X/month vs. traditional server infrastructure
- **Developer velocity**: 3x faster deployments with CI/CD automation

## Challenges & Solutions

### Challenge 1: Cold Start Latency

**Problem**: Lambda cold starts caused 3-5 second delays for first requests.

**Solution**:
```python
# Provisioned concurrency for critical functions
functions:
  emailClassifier:
    provisionedConcurrency: 2  # Always-warm instances

# Model caching to avoid S3 downloads
@cached(ttl=3600)
def load_model():
    return torch.load("/tmp/model.pt")
```

### Challenge 2: ZIP File Processing

**Problem**: Recursive ZIP extraction could cause infinite loops or memory exhaustion.

**Solution**:
```python
def safe_extract_zip(zip_file, max_depth=3, max_files=100):
    extracted_files = []

    def extract_recursive(zip_path, current_depth=0):
        if current_depth >= max_depth:
            logger.warning(f"Max depth reached: {zip_path}")
            return

        if len(extracted_files) >= max_files:
            logger.warning(f"Max files reached")
            return

        with zipfile.ZipFile(zip_path) as zf:
            for member in zf.namelist():
                # Security: Prevent path traversal
                if member.startswith("..") or member.startswith("/"):
                    continue

                extracted_path = zf.extract(member, "/tmp/extract")
                extracted_files.append(extracted_path)

                # Recursively extract nested ZIPs
                if member.endswith(".zip"):
                    extract_recursive(extracted_path, current_depth + 1)

    extract_recursive(zip_file)
    return extracted_files
```

### Challenge 3: Multilingual Response Quality

**Problem**: GPT-4's Thai and Japanese outputs sometimes lacked business formality.

**Solution**:
```python
# Language-specific system prompts
SYSTEM_PROMPTS = {
    "en": "You are a professional business assistant. Use formal English.",
    "ja": "あなたはビジネスメールの専門家です。丁寧な敬語を使用してください。",
    "th": "คุณเป็นผู้ช่วยธุรกิจมืออาชีพ กรุณาใช้ภาษาที่เป็นทางการ"
}

def generate_response(language, content):
    return llm.invoke([
        {"role": "system", "content": SYSTEM_PROMPTS[language]},
        {"role": "user", "content": content}
    ])
```

## Team Collaboration

**6-person team structure**:
- **Backend (2)**: Email API, DynamoDB operations, SES integration
- **Frontend (2)**: React dashboard, chatbot UI, IoT visualization
- **AI/ML (1)**: Model training, LangChain workflows *(me)*
- **Infrastructure (2)**: AWS deployment, Terraform, CI/CD *(me + 1)*

**My contributions**:
- Designed and implemented entire AI pipeline (classification + chatbot)
- Architected serverless infrastructure with cost optimization
- Set up CI/CD automation with Serverless Framework + Jenkins
- Mentored team on AWS best practices and LangChain patterns

## Results & Recognition

- **Excellence Award** at Samsung SDS Corporate Partnership Project
- **85% classification accuracy** on production email data
- Successfully demonstrated serverless + AI integration at scale
- Real-world deployment handling thousands of emails per day

## Lessons Learned

### 1. Serverless is Perfect for Event-Driven Workloads

Email processing is inherently event-driven - messages arrive unpredictably. Serverless Lambda functions auto-scale from zero to thousands of concurrent executions without manual capacity planning.

### 2. LangChain Simplifies LLM Workflows

Without LangChain, managing conversation state and tool-calling logic would require hundreds of lines of boilerplate. LangGraph's state machine abstraction made complex workflows readable.

### 3. CI/CD is Non-Negotiable

With 6 developers committing daily, automated testing and deployment prevented merge conflicts and production bugs. Serverless Framework's declarative config made infrastructure changes reviewable via Git.

### 4. Cost Monitoring is Critical

AWS Lambda bills per millisecond. Without CloudWatch alarms and cost budgets, a bug causing infinite retries could rack up unexpected charges. We implemented:
- Per-function cost tracking with tags
- Dead-letter queues to catch failures
- Timeout limits on all Lambda functions

## Future Enhancements

- **Voice-to-email**: Integrate with Amazon Transcribe for phone call summaries
- **Sentiment analysis**: Flag angry customer emails for priority handling
- **Predictive analytics**: Forecast quote acceptance rates using historical data
- **Multi-tenancy**: SaaS version for other companies

## Try It Out

Source code: [GitHub - samsungSDS_BusyBee](https://github.com/HyunjoJung/samsungSDS_BusyBee)

Detailed documentation: [Notion - BusyBee Project](https://www.notion.so/149d156ae6ee8081b1c2ed1c411a91e6?pvs=21)

---

**Questions or feedback?** Connect with me on [GitHub](https://github.com/HyunjoJung)
