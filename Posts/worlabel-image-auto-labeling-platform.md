---
title: Worlabel - Scaling Image Auto-Labeling with React & YOLOv8
description: How we reduced 10,000-image upload time from 18 minutes to 90 seconds using Presigned URLs and optimized rendering with react-window virtualization.
date: 2024-10-15
tags:
  - React
  - YOLOv8
  - Performance
  - Spring Boot
  - FastAPI
category: Frontend
featured: true
---

# Worlabel: Web-Based Image Auto-Labeling Platform

> **Work-Life-Balance through Automation**
> Free yourself from tedious data labeling and focus on what matters

## Project Overview

Worlabel is an automated data labeling service designed to streamline the image annotation process for machine learning projects. The platform operates through an iterative improvement cycle:

1. **Manual labeling** - Users annotate a small dataset
2. **Model training** - YOLOv8 trains on labeled data
3. **Auto-labeling** - Model predicts labels for remaining images
4. **Review & refine** - Users correct mistakes and retrain

This continuous loop produces increasingly accurate models while dramatically reducing manual effort.

**Development Period**: August 19 - October 11, 2024

## Tech Stack

### Frontend
- **React** - Component-based UI
- **Zustand** - Lightweight state management
- **react-window** - Virtualized rendering for 1000+ images

### Backend
- **Spring Boot** - RESTful API services
- **JPA** - Database ORM
- **FastAPI** - High-performance AI inference server

### AI/ML
- **YOLOv8 (Ultralytics)** - Real-time object detection and segmentation
- **PyTorch** - Deep learning framework

### Infrastructure
- **MySQL** - Relational database for metadata
- **Redis** - Caching layer for session data
- **Amazon S3** - Object storage for images
- **Firebase** - Real-time notifications
- **Docker** - Containerized deployment
- **Jenkins** - CI/CD automation
- **Nginx** - Reverse proxy and load balancing
- **GPU Server** - CUDA-enabled training infrastructure

## My Contributions: Frontend Performance Optimization

As one of two frontend developers, I focused on solving critical performance bottlenecks that were degrading user experience.

### Challenge 1: Image Upload Performance

**Problem**: Uploading 10,000 images took 18 minutes

The initial implementation used synchronous file transfer:

```typescript
// ❌ Original approach - synchronous upload through server
async function uploadImages(files: File[]) {
    const formData = new FormData();

    files.forEach(file => {
        formData.append('images', file);  // All files sent to server
    });

    // Server receives files, uploads to S3, returns metadata
    // Network bottleneck: All data goes through server
    const response = await fetch('/api/images/upload', {
        method: 'POST',
        body: formData  // Massive payload
    });

    return response.json();
}
```

**Issues**:
- Server acts as middleman, doubling network transfer
- Large payload (gigabytes) causes timeouts
- No progress tracking for individual files
- Single point of failure

**Solution**: Presigned URL Architecture

```typescript
// ✅ Optimized approach - direct S3 upload with presigned URLs

interface PresignedUrlResponse {
    fileId: string;
    uploadUrl: string;
    fields: Record<string, string>;
}

async function uploadImages(files: File[]) {
    // Step 1: Request presigned URLs from server (lightweight)
    const presignedRequests = files.map(file => ({
        fileName: file.name,
        fileSize: file.size,
        contentType: file.type
    }));

    const presignedUrls: PresignedUrlResponse[] = await fetch(
        '/api/images/presigned-urls',
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(presignedRequests)
        }
    ).then(r => r.json());

    // Step 2: Upload directly to S3 in parallel
    const uploadPromises = files.map(async (file, index) => {
        const { uploadUrl, fields, fileId } = presignedUrls[index];

        const formData = new FormData();
        Object.entries(fields).forEach(([key, value]) => {
            formData.append(key, value);
        });
        formData.append('file', file);

        // Direct upload to S3 (bypasses our server)
        await fetch(uploadUrl, {
            method: 'POST',
            body: formData
        });

        return fileId;
    });

    // Wait for all uploads to complete
    const fileIds = await Promise.all(uploadPromises);

    // Step 3: Notify server that uploads finished
    await fetch('/api/images/confirm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fileIds })
    });

    return fileIds;
}
```

**Backend implementation**:

```java
@RestController
@RequestMapping("/api/images")
public class ImageController {

    @Autowired
    private AmazonS3 s3Client;

    @PostMapping("/presigned-urls")
    public List<PresignedUrlResponse> generatePresignedUrls(
        @RequestBody List<PresignedUrlRequest> requests
    ) {
        return requests.stream().map(req -> {
            String fileId = UUID.randomUUID().toString();
            String s3Key = "images/" + fileId + "/" + req.getFileName();

            // Generate S3 presigned POST URL (valid for 1 hour)
            Date expiration = new Date(System.currentTimeMillis() + 3600000);
            GeneratePresignedUrlRequest presignedRequest =
                new GeneratePresignedUrlRequest(bucketName, s3Key)
                    .withMethod(HttpMethod.PUT)
                    .withExpiration(expiration);

            URL presignedUrl = s3Client.generatePresignedUrl(presignedRequest);

            // Save metadata to database
            imageRepository.save(new Image(
                fileId,
                s3Key,
                req.getFileName(),
                req.getFileSize(),
                ImageStatus.PENDING
            ));

            return new PresignedUrlResponse(fileId, presignedUrl.toString());
        }).collect(Collectors.toList());
    }

    @PostMapping("/confirm")
    public ResponseEntity<?> confirmUploads(@RequestBody ConfirmRequest request) {
        // Mark images as uploaded in database
        request.getFileIds().forEach(fileId -> {
            Image image = imageRepository.findById(fileId)
                .orElseThrow(() -> new NotFoundException("Image not found"));

            image.setStatus(ImageStatus.READY);
            imageRepository.save(image);
        });

        return ResponseEntity.ok().build();
    }
}
```

**Results**:
- **Upload time**: 18 minutes → **90 seconds** (92% reduction)
- **Server bandwidth**: Reduced by 95% (only metadata, no image data)
- **Scalability**: Parallel uploads to S3 instead of sequential server processing
- **User experience**: Real-time progress bars for each file

### Challenge 2: Rendering 1000+ Images

**Problem**: Sluggish performance when rendering large datasets

The initial implementation rendered all images at once:

```typescript
// ❌ Original approach - render all rows immediately
function ImageTable({ images }: { images: Image[] }) {
    return (
        <table>
            <tbody>
                {images.map(image => (
                    <tr key={image.id}>
                        <td>
                            <img
                                src={image.thumbnailUrl}
                                alt={image.name}
                            />
                        </td>
                        <td>{image.name}</td>
                        <td>{image.labels.join(', ')}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}
```

**Issues**:
- **Initial load time**: 5-10 seconds to render 1000 rows
- **Memory usage**: All DOM nodes created upfront
- **Scroll performance**: Janky scrolling due to layout thrashing
- **Network**: Thousands of image requests fired simultaneously

**Solution**: Virtualized Rendering with react-window

```typescript
// ✅ Optimized approach - only render visible rows
import { FixedSizeList as List } from 'react-window';
import AutoSizer from 'react-virtualized-auto-sizer';

interface ImageRowProps {
    index: number;
    style: React.CSSProperties;
    data: Image[];
}

function ImageRow({ index, style, data }: ImageRowProps) {
    const image = data[index];

    return (
        <div style={style} className="image-row">
            <div className="image-cell">
                <img
                    src={image.thumbnailUrl}
                    alt={image.name}
                    loading="lazy"  // Native lazy loading
                />
            </div>
            <div className="name-cell">{image.name}</div>
            <div className="labels-cell">
                {image.labels.join(', ')}
            </div>
        </div>
    );
}

function VirtualizedImageTable({ images }: { images: Image[] }) {
    return (
        <AutoSizer>
            {({ height, width }) => (
                <List
                    height={height}
                    itemCount={images.length}
                    itemSize={80}  // Row height in pixels
                    width={width}
                    itemData={images}
                    overscanCount={5}  // Render 5 extra rows above/below viewport
                >
                    {ImageRow}
                </List>
            )}
        </AutoSizer>
    );
}
```

**How it works**:

1. **Viewport calculation**: Only renders ~15 rows (whatever fits on screen)
2. **Dynamic rendering**: As user scrolls, old rows unmount and new rows mount
3. **Constant DOM size**: Always ~15 DOM nodes regardless of dataset size
4. **Lazy image loading**: Only visible images fetch thumbnails

**Results**:
- **Initial render**: 5-10 seconds → **<100ms**
- **Scroll FPS**: 15-20 FPS → **60 FPS** (buttery smooth)
- **Memory usage**: Reduced by 90% (only visible rows in memory)
- **Network requests**: Staggered instead of all-at-once

### Visualization: Before vs After

**Upload Performance**:
```
Before (Synchronous):
[Client] ---[2GB images]---> [Server] ---[2GB images]---> [S3]
   └─ 18 minutes total

After (Presigned URLs):
[Client] ---[10KB metadata]---> [Server]
[Client] ---[2GB images]------> [S3] (parallel)
   └─ 90 seconds total
```

**Rendering Performance**:
```
Before (Full Render):
- DOM nodes: 1000 <tr> elements
- Images loaded: 1000 requests
- Scroll: Janky (15-20 FPS)

After (Virtualized):
- DOM nodes: ~15 visible <div> elements
- Images loaded: ~15 requests (+ overscan)
- Scroll: Smooth (60 FPS)
```

## Team Structure

**6-person team**:
- **Frontend (2)**: React UI, performance optimization *(me + 1)*
- **Backend (2)**: Spring Boot API, database design
- **AI (1)**: YOLOv8 training, model optimization
- **Infrastructure (1)**: Docker, Jenkins, GPU server management

## Key Features

### 1. Manual Segmentation

Interactive canvas-based annotation tool:

```typescript
function SegmentationCanvas({ image }: { image: Image }) {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const [points, setPoints] = useState<Point[]>([]);

    const handleMouseDown = (e: React.MouseEvent) => {
        const canvas = canvasRef.current!;
        const rect = canvas.getBoundingClientRect();

        const point = {
            x: e.clientX - rect.left,
            y: e.clientY - rect.top
        };

        setPoints(prev => [...prev, point]);
        drawPolygon(points.concat(point));
    };

    const completeSegmentation = () => {
        // Convert canvas polygon to YOLO format
        const yoloAnnotation = convertToYOLO(points, image.width, image.height);

        // Save to backend
        saveAnnotation(image.id, yoloAnnotation);
    };

    return (
        <canvas
            ref={canvasRef}
            width={image.width}
            height={image.height}
            onMouseDown={handleMouseDown}
        />
    );
}
```

### 2. Auto-Labeling with YOLOv8

Once enough manual labels exist, trigger auto-labeling:

```python
# FastAPI backend for YOLOv8 inference
from fastapi import FastAPI, File, UploadFile
from ultralytics import YOLO
import cv2
import numpy as np

app = FastAPI()
model = YOLO('best.pt')  # Load trained model

@app.post("/predict")
async def predict(file: UploadFile):
    # Read uploaded image
    contents = await file.read()
    nparr = np.frombuffer(contents, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    # Run inference
    results = model.predict(img, conf=0.5)

    # Extract bounding boxes and masks
    predictions = []
    for result in results:
        for box, mask in zip(result.boxes, result.masks):
            predictions.append({
                "class": int(box.cls),
                "confidence": float(box.conf),
                "bbox": box.xyxy.tolist(),
                "mask": mask.xy.tolist()  # Polygon points
            })

    return {"predictions": predictions}
```

### 3. Model Training Progress

Real-time training metrics via WebSocket:

```typescript
function TrainingMonitor() {
    const [metrics, setMetrics] = useState<TrainingMetrics | null>(null);

    useEffect(() => {
        const ws = new WebSocket('ws://localhost:8000/training/ws');

        ws.onmessage = (event) => {
            const data = JSON.parse(event.data);
            setMetrics(data);
        };

        return () => ws.close();
    }, []);

    if (!metrics) return <div>Waiting for training to start...</div>;

    return (
        <div className="training-monitor">
            <div>Epoch: {metrics.epoch} / {metrics.totalEpochs}</div>
            <div>Loss: {metrics.loss.toFixed(4)}</div>
            <div>mAP@50: {metrics.map50.toFixed(3)}</div>
            <div>mAP@50-95: {metrics.map5095.toFixed(3)}</div>

            <LineChart data={metrics.lossHistory} />
        </div>
    );
}
```

## AI Model Improvement: Training Data Size Impact

We conducted experiments to measure how training data quantity affects model accuracy:

### Experiment Setup
- **Dataset**: Custom object detection task
- **Model**: YOLOv8n (nano variant)
- **Hardware**: Single NVIDIA RTX 3090
- **Training**: 100 epochs, default hyperparameters

### Results

| Training Images | mAP@50 | mAP@50-95 | Inference Time |
|----------------|--------|-----------|----------------|
| 10             | 0.32   | 0.18      | 3ms           |
| 100            | 0.68   | 0.45      | 3ms           |
| 1000           | 0.89   | 0.67      | 3ms           |

**Key insights**:
- **10 images**: Severe overfitting, poor generalization
- **100 images**: Decent baseline, useful for prototyping
- **1000 images**: Production-ready accuracy for most use cases

**Visualization**:
The confusion matrices showed dramatic improvement from 10→100→1000 images, with false positives dropping significantly.

## Performance Impact Summary

### Backend Optimization

**Presigned URL Benefits**:
```
Network Traffic Reduction:
- Before: 2GB × 2 (client→server, server→S3) = 4GB
- After: 10KB metadata + 2GB (client→S3) = ~2GB
- Savings: 50% total bandwidth

Server CPU Savings:
- Before: Processing 10,000 multipart uploads
- After: Generating 10,000 presigned URLs
- Savings: 95% CPU time (signing is cheap)

Cost Reduction:
- EC2 data transfer: $0.09/GB
- Saved: ~$360/month (assuming 2TB/month uploads)
```

### Frontend Optimization

**react-window Benefits**:
```
Memory Usage:
- Before: ~500MB (1000 rows × 500KB avg)
- After: ~8MB (15 visible rows)
- Savings: 98% reduction

Render Performance:
- Before: 5000ms initial render
- After: 50ms initial render
- Improvement: 100x faster

Scroll Performance:
- Before: 15-20 FPS (janky)
- After: 60 FPS (smooth)
- Improvement: 3-4x better
```

## Database Schema

```sql
-- User projects
CREATE TABLE projects (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Images within projects
CREATE TABLE images (
    id VARCHAR(36) PRIMARY KEY,  -- UUID
    project_id BIGINT NOT NULL,
    s3_key VARCHAR(512) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL,
    width INT NOT NULL,
    height INT NOT NULL,
    status ENUM('PENDING', 'READY', 'LABELED', 'TRAINING') DEFAULT 'PENDING',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    INDEX idx_project_status (project_id, status)
);

-- Annotations (manual or auto-generated)
CREATE TABLE annotations (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    image_id VARCHAR(36) NOT NULL,
    class_id INT NOT NULL,
    polygon JSON NOT NULL,  -- Array of [x, y] points
    confidence FLOAT,  -- NULL for manual, 0-1 for auto
    is_manual BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (image_id) REFERENCES images(id) ON DELETE CASCADE,
    INDEX idx_image (image_id)
);

-- Model training runs
CREATE TABLE training_runs (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    project_id BIGINT NOT NULL,
    model_path VARCHAR(512),  -- S3 path to best.pt
    map50 FLOAT,
    map5095 FLOAT,
    training_images INT,
    epochs INT,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
);
```

## Lessons Learned

### 1. Presigned URLs are a Game-Changer

For any application dealing with large file uploads, presigned URLs should be the default approach:
- Reduces server load by 95%
- Scales infinitely (S3 handles the load)
- Simpler error handling (client retries directly to S3)

### 2. Virtualization is Essential for Large Lists

react-window (or similar) should be used whenever rendering >50 items with images:
- Constant performance regardless of dataset size
- Works seamlessly with infinite scroll
- Low adoption cost (drop-in replacement for map)

### 3. Early Performance Testing Saves Time

We discovered the 18-minute upload problem after building most features. If we'd load-tested earlier with realistic data, we could have avoided the costly rewrite.

### 4. WebSocket for Real-Time Updates

Firebase worked well for notifications, but WebSocket would have been cheaper and given us more control over message format.

## Future Enhancements

- **Active learning**: Prioritize which images to label next based on model uncertainty
- **Multi-format export**: Support COCO, Pascal VOC, TFRecord formats
- **Collaborative labeling**: Multiple users annotating the same project
- **Version control**: Track annotation changes over time

## Try It Out

Source code: [GitHub - worlabel](https://github.com/HyunjoJung/worlabel)

---

**Questions or feedback?** Connect with me on [GitHub](https://github.com/HyunjoJung)
