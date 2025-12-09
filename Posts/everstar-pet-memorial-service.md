---
title: EVER-STAR - A Pet Memorial Service for Healing from Pet Loss
description: Building a compassionate digital space for pet owners to cope with grief through memorial creation, quest completion, and letter writing in TypeScript microservices architecture.
date: 2025-01-15
tags:
  - TypeScript
  - Microservices
  - Full-Stack
  - Mental Health
category: Full-Stack
featured: false
---

# EVER-STAR: Helping Pet Owners Heal from Pet Loss Syndrome

> A memorial space called 'Eternal Star' where pet owners can overcome pet loss syndrome through quests and letter writing on 'Earth Star', supporting their journey back to daily life

## Project Overview

EVER-STAR is a compassionate digital platform designed to help pet owners cope with the profound grief of losing a beloved companion. Pet loss syndrome - the psychological distress following a pet's death - affects millions of people worldwide, often dismissed or minimized by society.

Our service provides two interconnected spaces:

1. **Eternal Star (영원별)**: A memorial space where users create permanent tributes to their departed pets
2. **Earth Star (지구별)**: An interactive healing space with quests and letter-writing activities to facilitate healthy grieving

The platform helps users process their emotions, connect with others who understand their pain, and gradually return to daily life while honoring their pet's memory.

## Architecture: TypeScript Microservices

EVER-STAR is built with a microservices architecture, with each service handling a specific domain:

```
everStarFrontend/     - Next.js/React frontend
everStarBackAuth/     - Authentication & user management
everStarBackMain/     - Core features (memorials, profiles)
everStarBackChat/     - Real-time chat & community
```

### Why Microservices?

**Independent deployment**: Update chat features without touching memorial logic

**Technology flexibility**: Use different databases per service (PostgreSQL for auth, MongoDB for chat)

**Team scalability**: Multiple developers work on different services without conflicts

**Fault isolation**: If chat goes down, memorials remain accessible

## Tech Stack

### Frontend
- **Next.js** - Server-side rendering for SEO (important for memorial pages)
- **React** - Component-based UI
- **TypeScript** - Type safety across frontend/backend
- **TailwindCSS** - Utility-first styling

### Backend Services
- **Node.js + Express** - RESTful APIs
- **TypeScript** - Shared type definitions
- **PostgreSQL** - Relational data (users, memorials)
- **MongoDB** - Flexible schema (chat messages, activity logs)
- **Redis** - Session management & caching

### Infrastructure
- **Docker** - Containerized services
- **Nginx** - Reverse proxy and load balancing
- **PM2** - Process management for Node.js services

## Key Features

### 1. Eternal Star: Memorial Creation

**Creating a lasting tribute**:

```typescript
// Backend: Memorial creation endpoint
interface Memorial {
    id: string;
    petName: string;
    petSpecies: 'dog' | 'cat' | 'other';
    petBreed?: string;
    birthDate: Date;
    passedDate: Date;
    bio: string;
    photos: string[];  // URLs to uploaded images
    ownerId: string;
    visibility: 'public' | 'private' | 'friends-only';
    createdAt: Date;
}

// everStarBackMain/routes/memorials.ts
router.post('/memorials', auth, async (req: Request, res: Response) => {
    const {
        petName,
        petSpecies,
        birthDate,
        passedDate,
        bio,
        photos,
        visibility
    } = req.body;

    // Validate dates
    if (new Date(passedDate) < new Date(birthDate)) {
        return res.status(400).json({
            error: 'Passed date cannot be before birth date'
        });
    }

    // Create memorial in database
    const memorial = await Memorial.create({
        petName,
        petSpecies,
        birthDate,
        passedDate,
        bio,
        photos,
        ownerId: req.user.id,
        visibility
    });

    // Generate unique memorial URL
    const memorialUrl = `https://everstar.com/memorial/${memorial.id}`;

    res.status(201).json({
        memorial,
        url: memorialUrl
    });
});
```

**Frontend: Memorial page with photo carousel**:

```tsx
// everStarFrontend/pages/memorial/[id].tsx
import { useState } from 'react';
import Image from 'next/image';

interface MemorialPageProps {
    memorial: Memorial;
}

export default function MemorialPage({ memorial }: MemorialPageProps) {
    const [currentPhotoIndex, setCurrentPhotoIndex] = useState(0);

    const yearsLived = calculateYears(memorial.birthDate, memorial.passedDate);

    return (
        <div className="memorial-container">
            {/* Header with starry background */}
            <div className="eternal-star-header">
                <h1 className="pet-name">{memorial.petName}</h1>
                <p className="life-span">
                    {memorial.birthDate.toLocaleDateString()} -
                    {memorial.passedDate.toLocaleDateString()}
                </p>
                <p className="years-loved">{yearsLived} years of love</p>
            </div>

            {/* Photo carousel */}
            <div className="photo-carousel">
                <Image
                    src={memorial.photos[currentPhotoIndex]}
                    alt={`${memorial.petName} photo ${currentPhotoIndex + 1}`}
                    width={800}
                    height={600}
                    priority
                />

                <div className="carousel-controls">
                    {memorial.photos.map((_, index) => (
                        <button
                            key={index}
                            onClick={() => setCurrentPhotoIndex(index)}
                            className={index === currentPhotoIndex ? 'active' : ''}
                        />
                    ))}
                </div>
            </div>

            {/* Pet's story */}
            <div className="pet-bio">
                <h2>Their Story</h2>
                <p>{memorial.bio}</p>
            </div>

            {/* Condolence messages */}
            <CondolenceSection memorialId={memorial.id} />

            {/* Virtual candle lighting */}
            <CandleSection memorialId={memorial.id} />
        </div>
    );
}

// Server-side data fetching for SEO
export async function getServerSideProps({ params }) {
    const memorial = await fetch(
        `${process.env.API_URL}/memorials/${params.id}`
    ).then(r => r.json());

    return { props: { memorial } };
}
```

### 2. Earth Star: Healing Quests

**Quest system for gradual recovery**:

```typescript
// Quest types designed by grief counselors
interface Quest {
    id: string;
    title: string;
    description: string;
    category: 'reflection' | 'action' | 'connection' | 'remembrance';
    difficulty: 'easy' | 'medium' | 'hard';
    points: number;
    estimatedTime: number;  // minutes
}

const healingQuests: Quest[] = [
    {
        id: 'write-favorite-memory',
        title: 'Write Your Favorite Memory',
        description: 'Describe your happiest moment together in detail. What made it special?',
        category: 'reflection',
        difficulty: 'easy',
        points: 10,
        estimatedTime: 15
    },
    {
        id: 'create-photo-album',
        title: 'Create a Digital Photo Album',
        description: 'Compile 10-20 photos that capture your pet\'s personality',
        category: 'remembrance',
        difficulty: 'medium',
        points: 25,
        estimatedTime: 30
    },
    {
        id: 'walk-favorite-route',
        title: 'Walk Their Favorite Route',
        description: 'Take a walk along your pet\'s favorite path and notice the small things they loved',
        category: 'action',
        difficulty: 'hard',
        points: 50,
        estimatedTime: 60
    },
    {
        id: 'join-support-group',
        title: 'Connect with Others',
        description: 'Join a group chat with others who have experienced pet loss',
        category: 'connection',
        difficulty: 'medium',
        points: 30,
        estimatedTime: 20
    }
];

// Backend: Quest completion tracking
router.post('/quests/:questId/complete', auth, async (req, res) => {
    const { questId } = req.params;
    const { reflectionText, attachments } = req.body;

    // Record completion
    const completion = await QuestCompletion.create({
        userId: req.user.id,
        questId,
        reflectionText,
        attachments,
        completedAt: new Date()
    });

    // Award points
    const quest = healingQuests.find(q => q.id === questId);
    await User.increment('points', {
        by: quest.points,
        where: { id: req.user.id }
    });

    // Check for level up
    const user = await User.findByPk(req.user.id);
    const newLevel = calculateLevel(user.points);

    if (newLevel > user.level) {
        await user.update({ level: newLevel });

        // Unlock new quests at higher levels
        const unlockedQuests = getQuestsForLevel(newLevel);

        return res.json({
            completion,
            levelUp: true,
            newLevel,
            unlockedQuests
        });
    }

    res.json({ completion });
});

function calculateLevel(points: number): number {
    // Exponential level progression
    return Math.floor(Math.sqrt(points / 10)) + 1;
}
```

**Frontend: Quest interface**:

```tsx
// everStarFrontend/components/QuestBoard.tsx
import { useState, useEffect } from 'react';

interface QuestBoardProps {
    userId: string;
}

export default function QuestBoard({ userId }: QuestBoardProps) {
    const [activeQuests, setActiveQuests] = useState<Quest[]>([]);
    const [completedQuests, setCompletedQuests] = useState<string[]>([]);

    useEffect(() => {
        // Fetch user's quest progress
        fetch(`/api/users/${userId}/quests`)
            .then(r => r.json())
            .then(data => {
                setActiveQuests(data.available);
                setCompletedQuests(data.completed.map(q => q.questId));
            });
    }, [userId]);

    const handleQuestClick = (quest: Quest) => {
        // Open quest detail modal
        setSelectedQuest(quest);
    };

    return (
        <div className="quest-board">
            <h2>Your Healing Journey</h2>

            {/* Quest categories */}
            {['reflection', 'action', 'connection', 'remembrance'].map(category => (
                <div key={category} className="quest-category">
                    <h3>{category}</h3>

                    <div className="quest-grid">
                        {activeQuests
                            .filter(q => q.category === category)
                            .map(quest => (
                                <QuestCard
                                    key={quest.id}
                                    quest={quest}
                                    completed={completedQuests.includes(quest.id)}
                                    onClick={() => handleQuestClick(quest)}
                                />
                            ))}
                    </div>
                </div>
            ))}
        </div>
    );
}

function QuestCard({ quest, completed, onClick }) {
    return (
        <div
            className={`quest-card ${completed ? 'completed' : ''}`}
            onClick={onClick}
        >
            <div className="quest-header">
                <h4>{quest.title}</h4>
                {completed && <span className="checkmark">✓</span>}
            </div>

            <p className="quest-description">{quest.description}</p>

            <div className="quest-footer">
                <span className="points">+{quest.points} pts</span>
                <span className="time">{quest.estimatedTime} min</span>
                <span className={`difficulty ${quest.difficulty}`}>
                    {quest.difficulty}
                </span>
            </div>
        </div>
    );
}
```

### 3. Letter Writing

**Therapeutic letter writing feature**:

```typescript
// Backend: Letter storage (private, never shared without consent)
interface Letter {
    id: string;
    userId: string;
    petId: string;  // Link to memorial
    content: string;
    mood: 'sad' | 'nostalgic' | 'grateful' | 'peaceful';
    isPrivate: boolean;
    createdAt: Date;
}

router.post('/letters', auth, async (req, res) => {
    const { petId, content, mood, isPrivate } = req.body;

    const letter = await Letter.create({
        userId: req.user.id,
        petId,
        content,
        mood,
        isPrivate: isPrivate ?? true  // Default to private
    });

    // Optional: Suggest related quests based on mood
    const suggestedQuests = getQuestsForMood(mood);

    res.json({
        letter,
        suggestedQuests
    });
});

// Mood-based quest recommendations
function getQuestsForMood(mood: string): Quest[] {
    const moodQuestMap = {
        'sad': ['join-support-group', 'write-favorite-memory'],
        'nostalgic': ['create-photo-album', 'walk-favorite-route'],
        'grateful': ['donate-to-shelter', 'write-thank-you'],
        'peaceful': ['plant-memorial-tree', 'create-artwork']
    };

    const questIds = moodQuestMap[mood] || [];
    return healingQuests.filter(q => questIds.includes(q.id));
}
```

**Frontend: Letter writing interface**:

```tsx
// everStarFrontend/components/LetterWriter.tsx
import { useState } from 'react';
import { useRouter } from 'next/router';

interface LetterWriterProps {
    petName: string;
    petId: string;
}

export default function LetterWriter({ petName, petId }: LetterWriterProps) {
    const [content, setContent] = useState('');
    const [mood, setMood] = useState<string | null>(null);
    const [isPrivate, setIsPrivate] = useState(true);
    const router = useRouter();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const response = await fetch('/api/letters', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                petId,
                content,
                mood,
                isPrivate
            })
        });

        const data = await response.json();

        // Show success message and suggested quests
        router.push(`/letters/${data.letter.id}?suggestions=true`);
    };

    return (
        <form onSubmit={handleSubmit} className="letter-writer">
            <h2>Write to {petName}</h2>

            <p className="prompt">
                Take your time to express your thoughts and feelings.
                There's no right or wrong way to write this letter.
            </p>

            {/* Mood selector */}
            <div className="mood-selector">
                <label>How are you feeling today?</label>
                <div className="mood-options">
                    {['sad', 'nostalgic', 'grateful', 'peaceful'].map(m => (
                        <button
                            key={m}
                            type="button"
                            onClick={() => setMood(m)}
                            className={mood === m ? 'selected' : ''}
                        >
                            {getMoodEmoji(m)} {m}
                        </button>
                    ))}
                </div>
            </div>

            {/* Letter content */}
            <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder={`Dear ${petName},\n\nI wanted to tell you...`}
                rows={15}
                required
            />

            {/* Privacy toggle */}
            <label className="privacy-toggle">
                <input
                    type="checkbox"
                    checked={isPrivate}
                    onChange={(e) => setIsPrivate(e.target.checked)}
                />
                Keep this letter private (only visible to you)
            </label>

            <button
                type="submit"
                disabled={!content || !mood}
                className="submit-button"
            >
                Send Letter to {petName}
            </button>
        </form>
    );
}

function getMoodEmoji(mood: string): string {
    const emojiMap = {
        'sad': '😢',
        'nostalgic': '🌸',
        'grateful': '💖',
        'peaceful': '🕊️'
    };
    return emojiMap[mood] || '💭';
}
```

### 4. Real-Time Chat (Community Support)

**WebSocket-based group chat**:

```typescript
// everStarBackChat/server.ts
import { Server } from 'socket.io';
import { createServer } from 'http';

const httpServer = createServer();
const io = new Server(httpServer, {
    cors: {
        origin: process.env.FRONTEND_URL,
        credentials: true
    }
});

// Chat room management
const chatRooms = new Map<string, Set<string>>();  // roomId -> Set of userIds

io.on('connection', (socket) => {
    console.log(`User connected: ${socket.id}`);

    // Join a support group
    socket.on('join-room', async ({ roomId, userId }) => {
        socket.join(roomId);

        if (!chatRooms.has(roomId)) {
            chatRooms.set(roomId, new Set());
        }
        chatRooms.get(roomId).add(userId);

        // Notify others
        socket.to(roomId).emit('user-joined', {
            userId,
            timestamp: new Date()
        });

        // Send recent message history
        const recentMessages = await ChatMessage.findAll({
            where: { roomId },
            limit: 50,
            order: [['createdAt', 'DESC']]
        });

        socket.emit('message-history', recentMessages.reverse());
    });

    // Send message
    socket.on('send-message', async ({ roomId, userId, content }) => {
        // Save to database
        const message = await ChatMessage.create({
            roomId,
            userId,
            content,
            createdAt: new Date()
        });

        // Broadcast to room
        io.to(roomId).emit('new-message', {
            id: message.id,
            userId,
            content,
            timestamp: message.createdAt
        });
    });

    // Leave room
    socket.on('leave-room', ({ roomId, userId }) => {
        socket.leave(roomId);

        if (chatRooms.has(roomId)) {
            chatRooms.get(roomId).delete(userId);
        }

        socket.to(roomId).emit('user-left', {
            userId,
            timestamp: new Date()
        });
    });

    socket.on('disconnect', () => {
        console.log(`User disconnected: ${socket.id}`);
    });
});

httpServer.listen(3002, () => {
    console.log('Chat service running on port 3002');
});
```

## Microservices Communication

**Service-to-service authentication**:

```typescript
// everStarBackAuth/middleware/serviceAuth.ts
import jwt from 'jsonwebtoken';

const SERVICE_SECRET = process.env.SERVICE_SECRET;

// Generate service token for inter-service communication
export function generateServiceToken(serviceName: string): string {
    return jwt.sign(
        { service: serviceName, type: 'service' },
        SERVICE_SECRET,
        { expiresIn: '5m' }
    );
}

// Verify service token
export function verifyServiceToken(token: string): boolean {
    try {
        const decoded = jwt.verify(token, SERVICE_SECRET);
        return decoded.type === 'service';
    } catch {
        return false;
    }
}

// Example: Main service calling Auth service
// everStarBackMain/services/userService.ts
async function getUserPermissions(userId: string): Promise<string[]> {
    const serviceToken = generateServiceToken('main-service');

    const response = await fetch(
        `${process.env.AUTH_SERVICE_URL}/users/${userId}/permissions`,
        {
            headers: {
                'Authorization': `Bearer ${serviceToken}`,
                'X-Service-Name': 'main-service'
            }
        }
    );

    return response.json();
}
```

## Deployment Architecture

**Docker Compose setup**:

```yaml
# docker-compose.yml
version: '3.8'

services:
  frontend:
    build: ./everStarFrontend
    ports:
      - "3000:3000"
    environment:
      - API_URL=http://nginx:80
    depends_on:
      - nginx

  auth-service:
    build: ./everStarBackAuth
    ports:
      - "3001:3001"
    environment:
      - DATABASE_URL=postgresql://user:pass@postgres:5432/everstar_auth
      - JWT_SECRET=${JWT_SECRET}
      - SERVICE_SECRET=${SERVICE_SECRET}
    depends_on:
      - postgres

  main-service:
    build: ./everStarBackMain
    ports:
      - "3002:3002"
    environment:
      - DATABASE_URL=postgresql://user:pass@postgres:5432/everstar_main
      - AUTH_SERVICE_URL=http://auth-service:3001
    depends_on:
      - postgres

  chat-service:
    build: ./everStarBackChat
    ports:
      - "3003:3003"
    environment:
      - MONGO_URL=mongodb://mongo:27017/everstar_chat
    depends_on:
      - mongo

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - auth-service
      - main-service
      - chat-service

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_USER=user
      - POSTGRES_PASSWORD=pass
    volumes:
      - postgres-data:/var/lib/postgresql/data

  mongo:
    image: mongo:6
    volumes:
      - mongo-data:/data/db

  redis:
    image: redis:7-alpine
    volumes:
      - redis-data:/data

volumes:
  postgres-data:
  mongo-data:
  redis-data:
```

**Nginx reverse proxy**:

```nginx
# nginx.conf
http {
    upstream auth {
        server auth-service:3001;
    }

    upstream main {
        server main-service:3002;
    }

    upstream chat {
        server chat-service:3003;
    }

    server {
        listen 80;

        # Route to services based on path
        location /api/auth/ {
            proxy_pass http://auth/;
        }

        location /api/main/ {
            proxy_pass http://main/;
        }

        location /api/chat/ {
            proxy_pass http://chat/;

            # WebSocket support
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
        }

        # Frontend
        location / {
            proxy_pass http://frontend:3000;
        }
    }
}
```

## Privacy & Security

### Data Protection

Given the sensitive nature of grief and memorial content:

```typescript
// Encryption for sensitive data
import crypto from 'crypto';

const ENCRYPTION_KEY = process.env.ENCRYPTION_KEY;  // 32-byte key
const ALGORITHM = 'aes-256-gcm';

function encrypt(text: string): { encrypted: string; iv: string; tag: string } {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipheriv(ALGORITHM, Buffer.from(ENCRYPTION_KEY, 'hex'), iv);

    let encrypted = cipher.update(text, 'utf8', 'hex');
    encrypted += cipher.final('hex');

    const tag = cipher.getAuthTag();

    return {
        encrypted,
        iv: iv.toString('hex'),
        tag: tag.toString('hex')
    };
}

function decrypt(encrypted: string, iv: string, tag: string): string {
    const decipher = crypto.createDecipheriv(
        ALGORITHM,
        Buffer.from(ENCRYPTION_KEY, 'hex'),
        Buffer.from(iv, 'hex')
    );

    decipher.setAuthTag(Buffer.from(tag, 'hex'));

    let decrypted = decipher.update(encrypted, 'hex', 'utf8');
    decrypted += decipher.final('utf8');

    return decrypted;
}

// Store encrypted letters
router.post('/letters', auth, async (req, res) => {
    const { content, ...rest } = req.body;

    // Encrypt letter content
    const { encrypted, iv, tag } = encrypt(content);

    const letter = await Letter.create({
        ...rest,
        content: encrypted,
        iv,
        tag,
        userId: req.user.id
    });

    res.json({ letter: { ...letter, content: '[encrypted]' } });
});

// Retrieve and decrypt
router.get('/letters/:id', auth, async (req, res) => {
    const letter = await Letter.findByPk(req.params.id);

    // Verify ownership
    if (letter.userId !== req.user.id) {
        return res.status(403).json({ error: 'Unauthorized' });
    }

    // Decrypt content
    const decrypted = decrypt(letter.content, letter.iv, letter.tag);

    res.json({ ...letter.toJSON(), content: decrypted });
});
```

## Impact & Purpose

### Why This Matters

Pet loss syndrome is a real and often underestimated form of grief:
- **Disenfranchised grief**: Society often dismisses pet loss as "just an animal"
- **Intense bond**: Pets provide unconditional love and daily companionship
- **Lack of support**: Few resources exist specifically for pet bereavement

EVER-STAR aims to:
- **Validate emotions**: Recognize that pet grief is legitimate and significant
- **Provide structure**: Quests offer a framework for the healing process
- **Build community**: Connect users who understand the unique pain of pet loss
- **Honor memory**: Create permanent, beautiful tributes

### User Privacy First

All features respect user privacy:
- **Default privacy**: Letters and reflections are private by default
- **Consent-based sharing**: Users explicitly opt-in to make content public
- **Encryption**: Sensitive data encrypted at rest
- **No data selling**: Memorial content is sacred, never monetized

## Lessons Learned

### 1. Microservices for Team Productivity

With a TypeScript-based microservices architecture, developers could work independently without stepping on each other's code. The auth team didn't block the chat team.

### 2. Empathy-Driven Design

We consulted with grief counselors to ensure quests weren't too pushy or dismissive. The pacing and tone needed to respect where users are in their grieving process.

### 3. PostgreSQL vs MongoDB

- **PostgreSQL** for structured data (users, memorials, quests) - strong consistency needed
- **MongoDB** for chat messages - flexible schema, high write throughput

The right tool for the right job, even within one application.

## Future Enhancements

- **Mobile app**: iOS/Android for on-the-go journaling
- **Audio messages**: Voice notes to deceased pets
- **Therapy integration**: Connect with licensed pet loss counselors
- **Memorial QR codes**: Physical markers linking to digital tributes

## Source Code

Repository: [GitHub - EVER-STAR](https://github.com/HyunjoJung/EVER-STAR) *(Private Archive)*

---

**If you're grieving the loss of a pet**: You're not alone. Your pain is real, and your pet's love mattered. Take all the time you need.

**Questions or feedback?** Connect with me on [GitHub](https://github.com/HyunjoJung)
