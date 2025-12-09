---
title: EVER-STAR - Atomic Design과 OpenVidu로 구축한 반려동물 추모 서비스
description: React 18, 265개 이상의 컴포넌트를 가진 Atomic Design 패턴, 실시간 화상 채팅, 페이지 넘김 애니메이션이 있는 인터랙티브 추모 책을 사용하여 반려동물 상실을 위한 치유 플랫폼 개발
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

# EVER-STAR: 반려동물 상실을 위한 디지털 치유 공간

> **영원별 (Eternal Star)** - 추모 공간
> **지구별 (Earth Star)** - 퀘스트와 편지를 통한 인터랙티브 치유 여정

## 프로젝트 개요

EVER-STAR는 반려동물의 죽음 이후 찾아오는 깊은 슬픔인 **펫로스 증후군**을 겪는 반려동물 보호자들을 돕기 위해 설계된 따뜻한 웹 플랫폼입니다. 이 서비스는 사용자가 다음을 할 수 있는 두 개의 상호 연결된 공간을 제공합니다:

1. **영구적인 추모 공간 만들기** - 인터랙티브 디지털 책으로
2. **치유 퀘스트 완료하기** - 퍼즐, 화상 통화, 편지 쓰기를 통해
3. **다른 사람들과 연결하기** - 반려동물을 잃은 독특한 고통을 이해하는 사람들과

**개발 기간**: 2024년 1월 - 2월 (8주)

**팀**: 개발자 5명 (프론트엔드 3, 백엔드 2)

## 나의 역할: 프론트엔드 개발자

세 명의 프론트엔드 개발자 중 한 명으로서 컴포넌트 아키텍처와 인터랙티브 기능 구축에 집중했습니다:

### 주요 책임사항
- **Atomic Design 구현** - 265개 이상의 React 컴포넌트
- **추모 책 시스템** - 실감나는 페이지 넘김 애니메이션
- **퀘스트 시스템** - 퍼즐 게임 및 화상 채팅 통합 포함
- **상태 관리** - Redux Toolkit + React Query
- **Storybook 문서화** - 컴포넌트 라이브러리

## 기술 스택 심층 분석

### 프론트엔드 아키텍처

**핵심 프레임워크**: React 18 (Create React App)

```json
// package.json - 주요 의존성
{
  "dependencies": {
    "react": "^18.3.1",
    "react-router-dom": "^6.24.1",

    // 상태 관리
    "@reduxjs/toolkit": "^2.2.6",
    "@tanstack/react-query": "^5.51.21",
    "redux-persist": "^6.0.0",

    // 스타일링
    "styled-components": "^6.1.12",
    "tailwindcss": "^3.4.0",
    "framer-motion": "^11.3.19",

    // 특수 기능
    "react-pageflip": "^2.0.3",  // 책 페이지 넘기기
    "konva": "^9.3.14",          // 퍼즐용 캔버스
    "headbreaker": "^3.0.0",     // 퍼즐 생성
    "openvidu-browser": "^2.30.1", // 화상 채팅

    // 실시간 통신
    "@stomp/stompjs": "^7.0.0",
    "sockjs-client": "^1.6.1",

    // PDF 생성
    "html2canvas": "^1.4.1",
    "jspdf": "^2.5.1",
    "@react-pdf/renderer": "^3.4.4",

    // UI 라이브러리
    "sweetalert2": "^11.12.4",
    "react-datepicker": "^7.3.0",
    "react-slick": "^0.30.2"
  }
}
```

### 백엔드 (Spring Boot 3.3.1)

**참고**: 프론트엔드에 집중했지만, 백엔드 이해가 통합에 도움이 되었습니다.

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

## Atomic Design 아키텍처

Atomic Design 방법론을 사용하여 265개 이상의 컴포넌트를 구성했습니다:

```
src/components/
├── atoms/              # 80개 이상의 기본 컴포넌트
│   ├── buttons/       # PrimaryButton, Toggle, Tag 등
│   ├── icons/         # Arrow, Chat, Profile 등
│   ├── symbols/       # Logo, Book, Letter, Rainbow 등
│   └── texts/         # Label, Message, LetterText 등
│
├── molecules/         # 60개 이상의 복합 컴포넌트
│   ├── cards/         # LetterCard, PostItCard, PetCard
│   ├── inputs/        # TextField, DatePicker, SearchBar
│   └── Footer/        # 네비게이션 푸터
│
├── organisms/         # 40개 이상의 복잡한 섹션
│   ├── headers/       # PageHeader, ProfileHeader
│   ├── forms/         # LoginForm, SignUpForm
│   └── lists/         # LetterList, QuestList
│
└── templates/         # 35개 이상의 페이지 레이아웃
    ├── EarthMain.tsx          # 지구별 홈
    ├── EverStarMain.tsx       # 영원별 홈
    ├── MemorialBook.tsx       # 플립북 추모
    ├── LetterWriteTemplate.tsx
    ├── QuestPuzzle.tsx
    └── QuestOpenviduTemplate.tsx
```

### Atomic Design을 선택한 이유?

**우리가 경험한 이점**:
- **재사용성**: 50개 이상의 화면에서 사용되는 버튼
- **일관성**: 모든 곳에서 동일한 `Avatar` 컴포넌트
- **팀 협업**: 컴포넌트 충돌 없음
- **Storybook 통합**: 쉬운 문서화

**컴포넌트 계층 구조 예시**:
```
Template: MemorialBook
  └─ Organism: BookPages
      └─ Molecule: PageContent
          ├─ Atom: Avatar
          ├─ Atom: Label
          └─ Atom: LetterText
```

## 내가 구축한 주요 기능

### 1. 페이지 넘김 애니메이션이 있는 추모 책

**과제**: 실감나는 책 읽기 경험 만들기

**해결책**: `react-pageflip` 라이브러리 사용

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

  // 추모 책 데이터 가져오기
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
        {/* 표지 */}
        <div className="page cover-page">
          <img src={avatarUrl} alt="Pet" />
          <h1>{bookData?.petName}</h1>
          <p>{bookData?.memorialDate}</p>
        </div>

        {/* 내용 페이지 */}
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

              {/* 하단 페이지 번호 */}
              <div className="page-footer">
                <span className="page-number">{index + 1}</span>
              </div>
            </div>
          </div>
        ))}

        {/* 뒷표지 */}
        <div className="page back-cover">
          <div className="memories-summary">
            <p>사랑으로 만들어진</p>
            <p>{bookData?.totalMemories}개의 추억이 보존됨</p>
          </div>
        </div>
      </HTMLFlipBook>

      {/* 네비게이션 컨트롤 */}
      <div className="book-controls">
        <button
          onClick={() => bookRef.current?.pageFlip().flipPrev()}
          disabled={currentPage === 0}
        >
          이전
        </button>

        <span className="page-indicator">
          {currentPage + 1} / {bookData?.pages.length} 페이지
        </span>

        <button
          onClick={() => bookRef.current?.pageFlip().flipNext()}
          disabled={currentPage === bookData?.pages.length - 1}
        >
          다음
        </button>
      </div>
    </div>
  );
};
```

**emotion으로 스타일링**:
```tsx
// 책을 위한 스타일 컴포넌트
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

    /* 종이 질감 */
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

**결과**: 사용자가 실제 사진 앨범처럼 추모 페이지를 넘길 수 있습니다!

### 2. 인터랙티브 퍼즐 퀘스트

**과제**: 슬픔 치유를 위한 매력적인 활동 만들기

**해결책**: Konva + Headbreaker를 사용한 캔버스 기반 직소 퍼즐

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

    // 반려동물 이미지에서 퍼즐 생성
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

      // 퍼즐 조각 생성
      const newPuzzle = canvas.autogenerate({
        horizontalPiecesCount: 4,
        verticalPiecesCount: 3,
        insertsGenerator: (x, y) => ({
          right: Math.random() > 0.5 ? 1 : -1,
          bottom: Math.random() > 0.5 ? 1 : -1
        })
      });

      // 조각 섞기
      newPuzzle.shuffleGrid();

      // 캔버스에 그리기
      newPuzzle.draw();

      // 완성 리스너
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
    // 퀘스트를 완료로 표시
    await markQuestComplete({
      questId: 'puzzle-quest-1',
      completionTime: Date.now()
    });

    // 성공 메시지 표시
    Swal.fire({
      title: '퀘스트 완료!',
      text: '추억 퍼즐을 완성했습니다',
      icon: 'success',
      confirmButtonText: '계속하기'
    });
  };

  return (
    <div className="puzzle-container">
      <h2>추억 퍼즐 퀘스트</h2>
      <p>사랑하는 반려동물의 이 사진을 맞춰보세요</p>

      <canvas
        ref={canvasRef}
        id="puzzle-canvas"
        width={800}
        height={600}
      />

      {isComplete && (
        <div className="completion-message">
          <h3>아름다운 추억이 보존되었습니다! 🌟</h3>
          <button onClick={() => router.push('/earth')}>
            지구별로 돌아가기
          </button>
        </div>
      )}
    </div>
  );
};
```

### 3. 실시간 화상 채팅 퀘스트

**과제**: 반려동물을 잃은 다른 사람들과 대화할 수 있게 하기

**해결책**: 그룹 화상 통화를 위한 OpenVidu 통합

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
    // OpenVidu 초기화
    OV.current = new OpenVidu();
    const newSession = OV.current.initSession();

    // 스트림 이벤트 구독
    newSession.on('streamCreated', (event) => {
      const subscriber = newSession.subscribe(event.stream, undefined);
      setSubscribers((prev) => [...prev, subscriber]);
    });

    newSession.on('streamDestroyed', (event) => {
      setSubscribers((prev) =>
        prev.filter((sub) => sub !== event.stream.streamManager)
      );
    });

    // 백엔드에서 토큰 가져오기
    const token = await getToken('grief-support-room');

    // 세션 연결
    await newSession.connect(token, { clientData: 'User' });

    // 자신의 비디오/오디오 게시
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
      <h2>지지 그룹 화상 통화</h2>

      <div className="video-grid">
        {/* 자신의 비디오 (게시자) */}
        {publisher && (
          <div className="video-wrapper own-video">
            <video
              ref={(video) => {
                if (video) publisher.addVideoElement(video);
              }}
              autoPlay
              playsInline
            />
            <span className="video-label">나</span>
          </div>
        )}

        {/* 다른 참가자 (구독자) */}
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
              참가자 {index + 1}
            </span>
          </div>
        ))}
      </div>

      {/* 컨트롤 */}
      <div className="video-controls">
        <button onClick={toggleAudio}>
          <MicrophoneIcon />
          {publisher?.stream.audioActive ? '음소거' : '음소거 해제'}
        </button>

        <button onClick={toggleVideo}>
          <VideoIcon />
          {publisher?.stream.videoActive ? '비디오 중지' : '비디오 시작'}
        </button>

        <button onClick={leaveSession} className="leave-button">
          <PhoneStopIcon />
          통화 종료
        </button>
      </div>
    </div>
  );
};
```

### 4. 편지 쓰기 시스템

**지구별 기능**: 떠난 반려동물에게 치유 편지 쓰기

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
    // 백엔드에 편지 저장
    const letter = await createLetter({
      petId: petDetails.id,
      content,
      mood,
      isPrivate: true  // 기본 비공개
    });

    // 성공 애니메이션 표시
    await Swal.fire({
      title: '편지 전송됨',
      text: `${petDetails.name}에게 보내는 편지가 저장되었습니다`,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false
    });

    // 편지 상세로 이동
    router.push(`/earth/letter/${letter.id}`);
  };

  return (
    <LetterContainer>
      <h2>{petDetails?.name}에게 편지 쓰기</h2>

      {/* 기분 선택 */}
      <MoodSelector>
        <label>지금 기분이 어떠신가요?</label>
        <div className="mood-buttons">
          {[
            { value: 'sad', emoji: '😢', label: '슬픔' },
            { value: 'nostalgic', emoji: '🌸', label: '그리움' },
            { value: 'grateful', emoji: '💖', label: '감사함' },
            { value: 'peaceful', emoji: '🕊️', label: '평온함' }
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

      {/* 편지지 */}
      <LetterPaper>
        <div className="letter-header">
          <p>사랑하는 {petDetails?.name}에게,</p>
        </div>

        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="당신의 생각과 감정을 적어보세요..."
          rows={15}
        />

        <div className="letter-footer">
          <p>사랑을 담아,</p>
          <p className="signature">당신의 가족이</p>
        </div>
      </LetterPaper>

      {/* 액션 */}
      <div className="letter-actions">
        <button onClick={() => router.back()} className="cancel">
          취소
        </button>
        <button
          onClick={handleSubmit}
          disabled={content.length < 10}
          className="submit"
        >
          편지 보내기
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

  /* 줄무늬 종이 효과 */
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
    line-height: 31px;  // 줄무늬와 맞춤
    resize: none;
    outline: none;
    color: #333;
  }
`;
```

## 상태 관리 전략

**Redux Toolkit** (전역 상태) + **React Query** (서버 상태):

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
    staleTime: 5 * 60 * 1000,  // 5분
    cacheTime: 10 * 60 * 1000  // 10분
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

**이 조합을 선택한 이유?**
- **Redux**: 사용자 인증, 현재 반려동물 선택, UI 상태
- **React Query**: 자동 캐싱 및 재페칭이 있는 API 호출
- **Redux Persist**: 세션 간 반려동물 선택 저장

## 실시간 기능

**STOMP WebSocket**을 통한 실시간 업데이트:

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
      // 이 반려동물에 대한 응원 메시지 구독
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

## 성능 최적화

### 1. React.lazy를 사용한 코드 분할

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

### 2. 이미지 최적화

```tsx
// 265개 이상의 컴포넌트에서 사용
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

## Storybook을 통한 컴포넌트 문서화

**모든 265개 컴포넌트 문서화**:

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
    children: '클릭하세요',
    size: 'medium',
    disabled: false
  }
};

export const Disabled: Story = {
  args: {
    children: '비활성화된 버튼',
    disabled: true
  }
};
```

**Storybook 실행**:
```bash
npm run storybook
# http://localhost:6006에서 열림
```

## 과제 및 해결책

### 과제 1: 책 페이지 성능

**문제**: 고해상도 이미지로 인한 플립 애니메이션 지연

**해결책**:
```tsx
// 렌더링 전 이미지 사전 로드
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

### 과제 2: OpenVidu 연결 안정성

**문제**: 모바일에서 화상 통화 끊김

**해결책**: 재연결 로직 구현
```tsx
const handleReconnect = async () => {
  try {
    await leaveSession();
    await new Promise((resolve) => setTimeout(resolve, 1000));
    await joinSession();
  } catch (error) {
    console.error('재연결 실패:', error);
  }
};

// 연결 문제 시 자동 재연결
session.on('connectionDestroyed', (event) => {
  if (event.reason === 'networkDisconnect') {
    handleReconnect();
  }
});
```

### 과제 3: 상태 지속성

**문제**: 새로고침 시 사용자가 반려동물 선택 잃음

**해결책**: Redux Persist
```tsx
// store/Store.ts
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';

const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['pet', 'auth']  // 이 슬라이스만 지속
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

## 배운 점

### 1. Atomic Design의 확장성

265개 이상의 컴포넌트로 구성이 중요했습니다:
- **명확한 네이밍**: 단순히 `Button`이 아닌 `PrimaryButton`
- **일관된 props**: 모든 버튼이 `size`, `disabled`, `onClick` 공유
- **Storybook 문서화**: 팀 협업에 필수적

### 2. React Query > 수동 페칭

React Query 이전:
```tsx
// 수동 캐싱 악몽
const [petData, setPetData] = useState(null);
const [loading, setLoading] = useState(true);

useEffect(() => {
  fetchPet(petId).then(setPetData).finally(() => setLoading(false));
}, [petId]);
```

React Query 이후:
```tsx
// 자동 캐싱, 재페칭, 에러 처리
const { data, isLoading } = useFetchPetDetails(petId);
```

### 3. TypeScript가 우리를 구했다

**타입 안정성으로 수많은 버그 방지**:
```tsx
// 컴파일 시 타입 에러 발견!
<MemorialBook avatarUrl={123} />  // ❌ Type 'number'를 type 'string'에 할당할 수 없음

// 올바른 사용
<MemorialBook avatarUrl={petDetails.avatarUrl} />  // ✅
```

## 프로젝트 통계

- **총 컴포넌트**: 265개 TSX 파일
- **코드 라인 수**: ~50,000줄 (프론트엔드만)
- **번들 크기**: 2.8MB (gzip 이전)
- **Lighthouse 점수**: 85 (성능), 100 (접근성)
- **개발 기간**: 8주 (프론트엔드 3명, 백엔드 2명)

## 향후 개선사항

- **모바일 앱**: React Native 버전
- **음성 메시지**: 오디오 편지 녹음
- **AI 슬픔 상담**: GPT 기반 챗봇
- **3D 추모**: Three.js 가상 공간

## 소스 코드

저장소: [GitHub - EVER-STAR](https://github.com/HyunjoJung/EVER-STAR) *(비공개 아카이브)*

---

**반려동물을 잃은 슬픔을 겪고 있는 모든 분들께**: 당신의 아픔은 정당합니다. 치유하는 데 필요한 모든 시간을 가지세요.

**질문이나 피드백이 있으신가요?** [GitHub](https://github.com/HyunjoJung)에서 연락해주세요
