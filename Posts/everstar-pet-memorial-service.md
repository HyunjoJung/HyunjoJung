---
title: EVER-STAR - Building a Pet Memorial Service with Atomic Design & OpenVidu
description: Developing a compassionate healing platform for pet loss using React 18, Atomic Design pattern with 265+ components, real-time video chat, and interactive memorial books with page-flip animations.
date: 2025-01-15
tags:
  - React
  - Atomic Design
  - OpenVidu
  - Redux Toolkit
  - Spring Boot
category: Frontend
featured: false
---

# EVER-STAR: A Digital Healing Space for Pet Loss

> **영원별 (Eternal Star)** - A memorial space
> **지구별 (Earth Star)** - An interactive healing journey with quests and letters

## Project Overview

EVER-STAR is a compassionate web platform designed to help pet owners cope with **pet loss syndrome** - the profound grief following a pet's death. The service provides two interconnected spaces where users can:

1. **Create permanent memorials** with interactive digital books
2. **Complete healing quests** through puzzles, video calls, and letter writing
3. **Connect with others** who understand the unique pain of losing a pet

**Development Period**: January - February 2024 (8 weeks)

**Team**: 5 developers (3 Frontend, 2 Backend)

## My Role: Frontend Developer

As one of three frontend developers, I focused on building the component architecture and interactive features:

### Key Responsibilities
- **Atomic Design implementation** with 265+ React components
- **Memorial book system** with realistic page-flip animations
- **Quest system** including puzzle games and video chat integration
- **State management** with Redux Toolkit + React Query
- **Storybook documentation** for component library

## Tech Stack Deep Dive

### Frontend Architecture

**Core Framework**: React 18 (Create React App)

```json
// package.json - Key dependencies
{
  "dependencies": {
    "react": "^18.3.1",
    "react-router-dom": "^6.24.1",

    // State Management
    "@reduxjs/toolkit": "^2.2.6",
    "@tanstack/react-query": "^5.51.21",
    "redux-persist": "^6.0.0",

    // Styling
    "styled-components": "^6.1.12",
    "tailwindcss": "^3.4.0",
    "framer-motion": "^11.3.19",

    // Special Features
    "react-pageflip": "^2.0.3",  // Book page turning
    "konva": "^9.3.14",          // Canvas for puzzles
    "headbreaker": "^3.0.0",     // Puzzle generation
    "openvidu-browser": "^2.30.1", // Video chat

    // Real-time Communication
    "@stomp/stompjs": "^7.0.0",
    "sockjs-client": "^1.6.1",

    // PDF Generation
    "html2canvas": "^1.4.1",
    "jspdf": "^2.5.1",
    "@react-pdf/renderer": "^3.4.4",

    // UI Libraries
    "sweetalert2": "^11.12.4",
    "react-datepicker": "^7.3.0",
    "react-slick": "^0.30.2"
  }
}
```

### Backend (Spring Boot 3.3.1)

**Note**: While I focused on frontend, understanding the backend helped with integration.

```gradle
// everStarBackAuth/build.gradle
dependencies {
    // Spring Boot 3.3.1
    implementation 'org.springframework.boot:spring-boot-starter-web'
    implementation 'org.springframework.boot:spring-boot-starter-data-jpa'

    // Security & OAuth2
    implementation 'org.springframework.boot:spring-boot-starter-security'
    implementation 'org.springframework.boot:spring-boot-starter-oauth2-client'

    // Database
    runtimeOnly 'com.mysql:mysql-connector-j'
    implementation 'org.springframework.boot:spring-boot-starter-data-redis'

    // Lombok
    compileOnly 'org.projectlombok:lombok'
}
```

## Atomic Design Architecture

We organized 265+ components using Atomic Design methodology:

```
src/components/
├── atoms/              # 80+ basic components
│   ├── buttons/       # PrimaryButton, Toggle, Tag, etc.
│   ├── icons/         # Arrow, Chat, Profile, etc.
│   ├── symbols/       # Logo, Book, Letter, Rainbow, etc.
│   └── texts/         # Label, Message, LetterText, etc.
│
├── molecules/         # 60+ composite components
│   ├── cards/         # LetterCard, PostItCard, PetCard
│   ├── inputs/        # TextField, DatePicker, SearchBar
│   └── Footer/        # Navigation footer
│
├── organisms/         # 40+ complex sections
│   ├── headers/       # PageHeader, ProfileHeader
│   ├── forms/         # LoginForm, SignUpForm
│   └── lists/         # LetterList, QuestList
│
└── templates/         # 35+ page layouts
    ├── EarthMain.tsx          # Earth Star home
    ├── EverStarMain.tsx       # Eternal Star home
    ├── MemorialBook.tsx       # Flip book memorial
    ├── LetterWriteTemplate.tsx
    ├── QuestPuzzle.tsx
    └── QuestOpenviduTemplate.tsx
```

### Why Atomic Design?

**Benefits we experienced**:
- **Reusability**: Buttons used across 50+ screens
- **Consistency**: Same `Avatar` component everywhere
- **Team collaboration**: No component conflicts
- **Storybook integration**: Easy documentation

**Example Component Hierarchy**:
```
Template: MemorialBook
  └─ Organism: BookPages
      └─ Molecule: PageContent
          ├─ Atom: Avatar
          ├─ Atom: Label
          └─ Atom: LetterText
```

## Key Features I Built

### 1. Memorial Book with Page-Flip Animation

**Challenge**: Create a realistic book-reading experience

**Solution**: Used `react-pageflip` library

```tsx
// components/templates/MemorialBook.tsx
import HTMLFlipBook from "react-pageflip";
import { useRef, useState } from "react";

interface MemorialBookProps {
  avatarUrl: string;
  isOwner: boolean;
}

export const MemorialBook: React.FC<MemorialBookProps> = ({
  avatarUrl,
  isOwner
}) => {
  const bookRef = useRef<any>(null);
  const [currentPage, setCurrentPage] = useState(0);

  // Fetch memorial book data
  const { data: bookData } = useFetchMemorialBook();

  const handlePageFlip = (e: any) => {
    setCurrentPage(e.data);
  };

  return (
    <div className="memorial-book-container">
      <HTMLFlipBook
        ref={bookRef}
        width={400}
        height={600}
        size="fixed"
        minWidth={300}
        maxWidth={800}
        minHeight={400}
        maxHeight={1000}
        drawShadow={true}
        flippingTime={1000}
        usePortrait={true}
        startZIndex={0}
        autoSize={false}
        maxShadowOpacity={0.5}
        showCover={true}
        mobileScrollSupport={true}
        onFlip={handlePageFlip}
        className="flip-book"
      >
        {/* Cover page */}
        <div className="page cover-page">
          <img src={avatarUrl} alt="Pet" />
          <h1>{bookData?.petName}</h1>
          <p>{bookData?.memorialDate}</p>
        </div>

        {/* Content pages */}
        {bookData?.pages.map((page, index) => (
          <div key={index} className="page">
            <div className="page-content">
              <h2>{page.title}</h2>
              {page.imageUrl && (
                <img
                  src={page.imageUrl}
                  alt={page.title}
                  className="page-image"
                />
              )}
              <p className="page-text">{page.content}</p>

              {/* Page number at bottom */}
              <div className="page-footer">
                <span className="page-number">{index + 1}</span>
              </div>
            </div>
          </div>
        ))}

        {/* Back cover */}
        <div className="page back-cover">
          <div className="memories-summary">
            <p>Created with love</p>
            <p>{bookData?.totalMemories} memories preserved</p>
          </div>
        </div>
      </HTMLFlipBook>

      {/* Navigation controls */}
      <div className="book-controls">
        <button
          onClick={() => bookRef.current?.pageFlip().flipPrev()}
          disabled={currentPage === 0}
        >
          Previous
        </button>

        <span className="page-indicator">
          Page {currentPage + 1} of {bookData?.pages.length}
        </span>

        <button
          onClick={() => bookRef.current?.pageFlip().flipNext()}
          disabled={currentPage === bookData?.pages.length - 1}
        >
          Next
        </button>
      </div>
    </div>
  );
};
```

**Styled with emotion**:
```tsx
// Styled components for book
const BookContainer = styled.div`
  perspective: 1500px;

  .flip-book {
    margin: 50px auto;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  }

  .page {
    background: linear-gradient(
      to bottom,
      #f4f1e8 0%,
      #ebe6d9 100%
    );
    padding: 40px;
    border: 1px solid #d4cfc0;

    /* Paper texture */
    background-image: url('data:image/svg+xml,...');
  }

  .cover-page {
    background: linear-gradient(
      135deg,
      #667eea 0%,
      #764ba2 100%
    );
    color: white;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
  }
`;
```

**Result**: Users can flip through memorial pages like a real photo album!

### 2. Interactive Puzzle Quest

**Challenge**: Create engaging grief-healing activities

**Solution**: Canvas-based jigsaw puzzle with Konva + Headbreaker

```tsx
// components/templates/QuestPuzzle.tsx
import { Stage, Layer, Image as KonvaImage } from 'react-konva';
import { Canvas, Puzzle } from 'headbreaker';
import { useEffect, useRef, useState } from 'react';

export const QuestPuzzle: React.FC = () => {
  const [puzzle, setPuzzle] = useState<Puzzle | null>(null);
  const [isComplete, setIsComplete] = useState(false);
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    if (!canvasRef.current) return;

    // Create puzzle from pet image
    const image = new window.Image();
    image.src = '/assets/pet-photo.jpg';

    image.onload = () => {
      const canvas = new Canvas(canvasRef.current!.id, {
        width: 800,
        height: 600,
        pieceSize: 100,
        proximity: 20,
        borderFill: 10,
        strokeWidth: 2,
        lineSoftness: 0.18
      });

      // Generate puzzle pieces
      const newPuzzle = canvas.autogenerate({
        horizontalPiecesCount: 4,
        verticalPiecesCount: 3,
        insertsGenerator: (x, y) => ({
          right: Math.random() > 0.5 ? 1 : -1,
          bottom: Math.random() > 0.5 ? 1 : -1
        })
      });

      // Shuffle pieces
      newPuzzle.shuffleGrid();

      // Draw on canvas
      newPuzzle.draw();

      // Listen for completion
      newPuzzle.onConnect((piece, target) => {
        if (newPuzzle.isValid()) {
          setIsComplete(true);
          onPuzzleComplete();
        }
      });

      setPuzzle(newPuzzle);
    };
  }, []);

  const onPuzzleComplete = async () => {
    // Mark quest as completed
    await markQuestComplete({
      questId: 'puzzle-quest-1',
      completionTime: Date.now()
    });

    // Show success message
    Swal.fire({
      title: 'Quest Complete!',
      text: 'You completed the memory puzzle',
      icon: 'success',
      confirmButtonText: 'Continue'
    });
  };

  return (
    <div className="puzzle-container">
      <h2>Memory Puzzle Quest</h2>
      <p>Piece together this photo of your beloved companion</p>

      <canvas
        ref={canvasRef}
        id="puzzle-canvas"
        width={800}
        height={600}
      />

      {isComplete && (
        <div className="completion-message">
          <h3>Beautiful memories preserved! 🌟</h3>
          <button onClick={() => router.push('/earth')}>
            Return to Earth Star
          </button>
        </div>
      )}
    </div>
  );
};
```

### 3. Real-Time Video Chat Quest

**Challenge**: Enable users to talk with others who've lost pets

**Solution**: OpenVidu integration for group video calls

```tsx
// components/templates/QuestOpenviduTemplate.tsx
import { OpenVidu, Session, StreamManager } from 'openvidu-browser';
import { useEffect, useRef, useState } from 'react';

export const QuestOpenviduTemplate: React.FC = () => {
  const [session, setSession] = useState<Session | null>(null);
  const [publisher, setPublisher] = useState<StreamManager | null>(null);
  const [subscribers, setSubscribers] = useState<StreamManager[]>([]);
  const OV = useRef<OpenVidu | null>(null);

  useEffect(() => {
    joinSession();

    return () => {
      leaveSession();
    };
  }, []);

  const joinSession = async () => {
    // Initialize OpenVidu
    OV.current = new OpenVidu();
    const newSession = OV.current.initSession();

    // Subscribe to stream events
    newSession.on('streamCreated', (event) => {
      const subscriber = newSession.subscribe(event.stream, undefined);
      setSubscribers((prev) => [...prev, subscriber]);
    });

    newSession.on('streamDestroyed', (event) => {
      setSubscribers((prev) =>
        prev.filter((sub) => sub !== event.stream.streamManager)
      );
    });

    // Get token from backend
    const token = await getToken('grief-support-room');

    // Connect to session
    await newSession.connect(token, { clientData: 'User' });

    // Publish own video/audio
    const newPublisher = await OV.current.initPublisherAsync(undefined, {
      audioSource: undefined,
      videoSource: undefined,
      publishAudio: true,
      publishVideo: true,
      resolution: '640x480',
      frameRate: 30,
      insertMode: 'APPEND',
      mirror: false
    });

    newSession.publish(newPublisher);

    setSession(newSession);
    setPublisher(newPublisher);
  };

  const leaveSession = () => {
    if (session) {
      session.disconnect();
    }
    setSession(null);
    setPublisher(null);
    setSubscribers([]);
  };

  const toggleAudio = () => {
    if (publisher) {
      publisher.publishAudio(!publisher.stream.audioActive);
    }
  };

  const toggleVideo = () => {
    if (publisher) {
      publisher.publishVideo(!publisher.stream.videoActive);
    }
  };

  return (
    <div className="video-chat-container">
      <h2>Support Group Video Call</h2>

      <div className="video-grid">
        {/* Own video (publisher) */}
        {publisher && (
          <div className="video-wrapper own-video">
            <video
              ref={(video) => {
                if (video) publisher.addVideoElement(video);
              }}
              autoPlay
              playsInline
            />
            <span className="video-label">You</span>
          </div>
        )}

        {/* Other participants (subscribers) */}
        {subscribers.map((sub, index) => (
          <div key={index} className="video-wrapper">
            <video
              ref={(video) => {
                if (video) sub.addVideoElement(video);
              }}
              autoPlay
              playsInline
            />
            <span className="video-label">
              Participant {index + 1}
            </span>
          </div>
        ))}
      </div>

      {/* Controls */}
      <div className="video-controls">
        <button onClick={toggleAudio}>
          <MicrophoneIcon />
          {publisher?.stream.audioActive ? 'Mute' : 'Unmute'}
        </button>

        <button onClick={toggleVideo}>
          <VideoIcon />
          {publisher?.stream.videoActive ? 'Stop Video' : 'Start Video'}
        </button>

        <button onClick={leaveSession} className="leave-button">
          <PhoneStopIcon />
          Leave Call
        </button>
      </div>
    </div>
  );
};
```

### 4. Letter Writing System

**Earth Star Feature**: Write therapeutic letters to departed pets

```tsx
// components/templates/LetterWriteTemplate.tsx
import { useState } from 'react';
import { useSelector } from 'react-redux';
import styled from 'styled-components';

export const LetterWriteTemplate: React.FC = () => {
  const petDetails = useSelector((state: RootState) => state.pet.petDetails);
  const [content, setContent] = useState('');
  const [mood, setMood] = useState<'sad' | 'nostalgic' | 'grateful' | 'peaceful'>('nostalgic');

  const handleSubmit = async () => {
    // Save letter to backend
    const letter = await createLetter({
      petId: petDetails.id,
      content,
      mood,
      isPrivate: true  // Default private
    });

    // Show success animation
    await Swal.fire({
      title: 'Letter Sent',
      text: `Your letter to ${petDetails.name} has been saved`,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false
    });

    // Navigate to letter detail
    router.push(`/earth/letter/${letter.id}`);
  };

  return (
    <LetterContainer>
      <h2>Write to {petDetails?.name}</h2>

      {/* Mood selector */}
      <MoodSelector>
        <label>How are you feeling?</label>
        <div className="mood-buttons">
          {[
            { value: 'sad', emoji: '😢', label: 'Sad' },
            { value: 'nostalgic', emoji: '🌸', label: 'Nostalgic' },
            { value: 'grateful', emoji: '💖', label: 'Grateful' },
            { value: 'peaceful', emoji: '🕊️', label: 'Peaceful' }
          ].map((option) => (
            <button
              key={option.value}
              onClick={() => setMood(option.value as any)}
              className={mood === option.value ? 'active' : ''}
            >
              <span className="emoji">{option.emoji}</span>
              <span>{option.label}</span>
            </button>
          ))}
        </div>
      </MoodSelector>

      {/* Letter paper */}
      <LetterPaper>
        <div className="letter-header">
          <p>Dear {petDetails?.name},</p>
        </div>

        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="Write your thoughts and feelings..."
          rows={15}
        />

        <div className="letter-footer">
          <p>With love,</p>
          <p className="signature">Your human</p>
        </div>
      </LetterPaper>

      {/* Actions */}
      <div className="letter-actions">
        <button onClick={() => router.back()} className="cancel">
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={content.length < 10}
          className="submit"
        >
          Send Letter
        </button>
      </div>
    </LetterContainer>
  );
};

const LetterPaper = styled.div`
  background: linear-gradient(to bottom, #fffef7 0%, #f7f4e8 100%);
  padding: 40px;
  border: 1px solid #d4cfc0;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  margin: 30px 0;

  /* Lined paper effect */
  background-image:
    repeating-linear-gradient(
      transparent,
      transparent 30px,
      #e8e3d3 30px,
      #e8e3d3 31px
    );

  textarea {
    width: 100%;
    border: none;
    background: transparent;
    font-family: 'Noto Sans KR', sans-serif;
    font-size: 16px;
    line-height: 31px;  // Match lined paper
    resize: none;
    outline: none;
    color: #333;
  }
`;
```

## State Management Strategy

**Redux Toolkit** for global state + **React Query** for server state:

```tsx
// store/petSlice.ts
import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface PetState {
  petDetails: PetDetails | null;
  selectedMemorialBookId: number | null;
}

const initialState: PetState = {
  petDetails: null,
  selectedMemorialBookId: null
};

export const petSlice = createSlice({
  name: 'pet',
  initialState,
  reducers: {
    setPetDetails: (state, action: PayloadAction<PetDetails>) => {
      state.petDetails = action.payload;
    },
    setMemorialBookId: (state, action: PayloadAction<number>) => {
      state.selectedMemorialBookId = action.payload;
    },
    clearPetData: (state) => {
      state.petDetails = null;
      state.selectedMemorialBookId = null;
    }
  }
});

export const { setPetDetails, setMemorialBookId, clearPetData } = petSlice.actions;
```

```tsx
// hooks/useEverStar.ts
import { useQuery } from '@tanstack/react-query';
import axios from 'axios';

export const useFetchOtherPetDetails = (petId: number) => {
  return useQuery({
    queryKey: ['petDetails', petId],
    queryFn: async () => {
      const { data } = await axios.get(`/api/pets/${petId}`);
      return data;
    },
    enabled: petId > 0,
    staleTime: 5 * 60 * 1000,  // 5 minutes
    cacheTime: 10 * 60 * 1000  // 10 minutes
  });
};

export const useFetchMemorialBooksWithQuest = (petId: number, questIndex: number) => {
  return useQuery({
    queryKey: ['memorialBooks', petId, questIndex],
    queryFn: async () => {
      const { data } = await axios.get(
        `/api/memorial-books?petId=${petId}&questIndex=${questIndex}`
      );
      return data;
    },
    enabled: petId > 0
  });
};
```

**Why this combination?**
- **Redux**: User auth, current pet selection, UI state
- **React Query**: API calls with automatic caching & refetching
- **Redux Persist**: Save pet selection across sessions

## Real-Time Features

**STOMP WebSocket** for live updates:

```tsx
// hooks/useWebSocket.ts
import { Client } from '@stomp/stompjs';
import SockJS from 'sockjs-client';
import { useEffect, useRef } from 'react';

export const useCheeringMessages = (petId: number) => {
  const clientRef = useRef<Client | null>(null);

  useEffect(() => {
    const socket = new SockJS('/ws');
    const stompClient = new Client({
      webSocketFactory: () => socket,
      reconnectDelay: 5000,
      heartbeatIncoming: 4000,
      heartbeatOutgoing: 4000
    });

    stompClient.onConnect = () => {
      // Subscribe to cheering messages for this pet
      stompClient.subscribe(`/topic/cheering/${petId}`, (message) => {
        const newMessage = JSON.parse(message.body);
        queryClient.invalidateQueries(['cheeringMessages', petId]);
      });
    };

    stompClient.activate();
    clientRef.current = stompClient;

    return () => {
      stompClient.deactivate();
    };
  }, [petId]);
};
```

## Performance Optimizations

### 1. Code Splitting with React.lazy

```tsx
// App.tsx
import { lazy, Suspense } from 'react';

const EarthPage = lazy(() => import('./pages/EarthPage'));
const EverstarPage = lazy(() => import('./pages/EverstarPage'));
const MyPage = lazy(() => import('./pages/MyPage'));

function App() {
  return (
    <Suspense fallback={<SplashTemplate />}>
      <Routes>
        <Route path="/earth/*" element={<EarthPage />} />
        <Route path="/everstar/:pet?" element={<EverstarPage />} />
        <Route path="/mypage" element={<MyPage />} />
      </Routes>
    </Suspense>
  );
}
```

### 2. Image Optimization

```tsx
// Used across 265+ components
const OptimizedImage: React.FC<{ src: string; alt: string }> = ({ src, alt }) => {
  const [loaded, setLoaded] = useState(false);

  return (
    <div className="image-container">
      {!loaded && <Skeleton />}
      <img
        src={src}
        alt={alt}
        onLoad={() => setLoaded(true)}
        loading="lazy"
        style={{ display: loaded ? 'block' : 'none' }}
      />
    </div>
  );
};
```

## Component Documentation with Storybook

**All 265 components documented**:

```tsx
// Button.stories.tsx
import type { Meta, StoryObj } from '@storybook/react';
import { PrimaryButton } from './PrimaryButton';

const meta: Meta<typeof PrimaryButton> = {
  title: 'Atoms/Buttons/PrimaryButton',
  component: PrimaryButton,
  tags: ['autodocs'],
  argTypes: {
    size: {
      control: 'select',
      options: ['small', 'medium', 'large']
    },
    disabled: {
      control: 'boolean'
    }
  }
};

export default meta;
type Story = StoryObj<typeof PrimaryButton>;

export const Default: Story = {
  args: {
    children: 'Click Me',
    size: 'medium',
    disabled: false
  }
};

export const Disabled: Story = {
  args: {
    children: 'Disabled Button',
    disabled: true
  }
};
```

**Run Storybook**:
```bash
npm run storybook
# Opens at http://localhost:6006
```

## Challenges & Solutions

### Challenge 1: Book Page Performance

**Problem**: Flip animations laggy with high-resolution images

**Solution**:
```tsx
// Preload images before rendering
const preloadImages = async (imageUrls: string[]) => {
  const promises = imageUrls.map((url) => {
    return new Promise((resolve) => {
      const img = new Image();
      img.src = url;
      img.onload = resolve;
    });
  });

  await Promise.all(promises);
};

useEffect(() => {
  if (bookData) {
    const images = bookData.pages.map((p) => p.imageUrl).filter(Boolean);
    preloadImages(images);
  }
}, [bookData]);
```

### Challenge 2: OpenVidu Connection Stability

**Problem**: Video calls dropping on mobile

**Solution**: Implement reconnection logic
```tsx
const handleReconnect = async () => {
  try {
    await leaveSession();
    await new Promise((resolve) => setTimeout(resolve, 1000));
    await joinSession();
  } catch (error) {
    console.error('Reconnection failed:', error);
  }
};

// Auto-reconnect on connection issues
session.on('connectionDestroyed', (event) => {
  if (event.reason === 'networkDisconnect') {
    handleReconnect();
  }
});
```

### Challenge 3: State Persistence

**Problem**: User loses pet selection on refresh

**Solution**: Redux Persist
```tsx
// store/Store.ts
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';

const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['pet', 'auth']  // Only persist these slices
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER]
      }
    })
});

export const persistor = persistStore(store);
```

## Lessons Learned

### 1. Atomic Design Scales Well

With 265+ components, organization was critical:
- **Clear naming**: `PrimaryButton`, not just `Button`
- **Consistent props**: All buttons share `size`, `disabled`, `onClick`
- **Storybook documentation**: Essential for team collaboration

### 2. React Query > Manual Fetching

Before React Query:
```tsx
// Manual caching nightmare
const [petData, setPetData] = useState(null);
const [loading, setLoading] = useState(true);

useEffect(() => {
  fetchPet(petId).then(setPetData).finally(() => setLoading(false));
}, [petId]);
```

After React Query:
```tsx
// Automatic caching, refetching, error handling
const { data, isLoading } = useFetchPetDetails(petId);
```

### 3. TypeScript Saved Us

**Type safety prevented countless bugs**:
```tsx
// Type error caught at compile time!
<MemorialBook avatarUrl={123} />  // ❌ Type 'number' is not assignable to type 'string'

// Correct usage
<MemorialBook avatarUrl={petDetails.avatarUrl} />  // ✅
```

## Project Statistics

- **Total Components**: 265 TSX files
- **Lines of Code**: ~50,000 (frontend only)
- **Bundle Size**: 2.8MB (pre-gzip)
- **Lighthouse Score**: 85 (Performance), 100 (Accessibility)
- **Development Time**: 8 weeks (3 Frontend, 2 Backend)

## Future Improvements

- **Mobile app**: React Native version
- **Voice messages**: Record audio letters
- **AI grief counseling**: GPT-powered chatbot
- **3D memorials**: Three.js virtual spaces

## Source Code

Repository: [GitHub - EVER-STAR](https://github.com/HyunjoJung/EVER-STAR) *(Private Archive)*

---

**For anyone grieving a pet**: Your pain is valid. Take all the time you need to heal.

**Questions or feedback?** Connect with me on [GitHub](https://github.com/HyunjoJung)
