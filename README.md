# 정현조 · Hyunjo Jung

> AI로 실제 운영되는 서비스를 만드는 개발자
> *Building real, in-production services with AI*

![Rust](https://img.shields.io/badge/Rust-000000?style=for-the-badge&logo=rust&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js-000000?style=for-the-badge&logo=nextdotjs&logoColor=white)
![AI / MCP](https://img.shields.io/badge/AI%20·%20MCP-blueviolet?style=for-the-badge&logo=anthropic&logoColor=white)
![SSAFY 11th](https://img.shields.io/badge/SSAFY_11th-Graduated-0f7cc0?style=for-the-badge&logo=samsung&logoColor=white)

---

## 💼 현재 · Now

- 🏢 **Prain Global** — IT/AI 담당 (사내 자동화·AI 도구, 모니터링 SaaS)
- 🇰🇷 **독립 공공데이터·AI 개발** — 조달청·한국관광공사 등 공공 공모전 출품작을 *실제 운영되는 서비스*로 구현
- 🧠 **관심** — 검색/RAG, MCP 기반 AI 연동, 온디바이스 LLM, 운영 가능한 백엔드 설계 (Rust)

---

## 🚀 대표 프로젝트 · Featured

### 🏛️ kr-bid — 나라장터 조달공고·첨부문서 AI 검색
조달청 입찰공고·낙찰 공공데이터와 **HWP/PDF/XLSX 첨부서류 본문**을 자동 수집·해석해, 자격조건·마감·낙찰 근거까지 한 흐름으로 찾아주는 **실서비스**. 비서형 AI가 MCP로 최신 조달 데이터를 직접 조회.

- 🌐 **서비스:** **[kr-bid.com](https://kr-bid.com)** (운영 중)
- 🤖 **AI 연결:** 카카오 PlayMCP 등록 — [playmcp.kakao.com/mcp/540](https://playmcp.kakao.com/mcp/540)
- 🛠️ **스택:** Rust (헥사고날 워크스페이스, Axum, SQLx), PostgreSQL, MCP, Next.js
- ✍️ **오픈소스 스핀오프:** 첨부문서 파서를 [`rwml`](https://crates.io/crates/rwml) 크레이트로 독립 — crates.io 공개 (↓ Featured)

### 🧭 kr-tour — 한국관광공사(KTO) 배지·코스 서비스
TourAPI + Supabase(PGroonga·pgvector·PostGIS) 기반 관광 추천/코스 서비스.
**KTO 2026 공모전 ①웹·앱 개발 부문 예비심사 합격작.**

### 🔗 SheetLink — 엑셀 하이퍼링크 추출·병합 도구
서버 저장 없는(프라이버시) 엑셀 링크 추출/병합 웹 도구.
- 🌐 **[sheetlink.hyunjo.uk](https://sheetlink.hyunjo.uk)** · ASP.NET Core / Blazor / OpenXML

### 📦 rwml — Microsoft Word `.doc`/`.docx` 네이티브 Rust 툴킷
kr-bid 첨부문서 파서에서 출발해, 레거시 `.doc`(Word 97–2003 바이너리)와 모던 `.docx`(OOXML)를 **하나의 모델로 읽기·쓰기·편집·PDF 렌더링**하는 native Rust 크레이트로 독립. JVM·Apache POI·서브프로세스 없이 `#![forbid(unsafe_code)]`·퍼징·XXE-safe. **crates.io 첫 공개 배포작.**
- 📦 **[crates.io/rwml](https://crates.io/crates/rwml)** · 📖 [docs.rs](https://docs.rs/rwml) · 🐙 [GitHub](https://github.com/HyunjoJung/rwml) · 🛠️ Rust · parley/krilla PDF

> 그 외 산업안전 온디바이스 VLM(safety-eye), 공정위 의결서 RAG(FairData Counsel),
> 가정용 화학제품 라벨 OCR Android 앱(LabelGuard) 등 공공데이터·AI 공모전 출품작 다수.

---

## 🧠 기술 스택 · Tech Stack

**백엔드/시스템** ![Rust](https://img.shields.io/badge/Rust-000000?style=flat-square&logo=rust&logoColor=white) ![Python](https://img.shields.io/badge/Python-3776AB?style=flat-square&logo=python&logoColor=white) ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white) ![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white)

**AI/데이터** ![MCP](https://img.shields.io/badge/MCP-blueviolet?style=flat-square) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=flat-square&logo=postgresql&logoColor=white) ![Supabase](https://img.shields.io/badge/Supabase-3FCF8E?style=flat-square&logo=supabase&logoColor=white) ![PyTorch](https://img.shields.io/badge/PyTorch-EE4C2C?style=flat-square&logo=pytorch&logoColor=white)

**프런트엔드** ![Next.js](https://img.shields.io/badge/Next.js-000000?style=flat-square&logo=nextdotjs&logoColor=white) ![React](https://img.shields.io/badge/React-61DAFB?style=flat-square&logo=react&logoColor=black) ![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white) ![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=flat-square&logo=blazor&logoColor=white)

**인프라** ![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white) ![AWS](https://img.shields.io/badge/AWS-232F3E?style=flat-square&logo=amazonaws&logoColor=white)

---

## 🏅 이력 · Highlights

| 시기 | 내용 |
|---|---|
| 2026 | 공공데이터·AI 공모전 출품 — **kr-bid**(조달청 창업경진대회), **kr-tour**(KTO 관광 공모전 **예비심사 합격**) |
| 2025.06 | **SQLD** 자격 |
| 2024.12 | **SSAFY 11기** 우수 수료 |
| 2024.11 | 삼성SDS 프로젝트 — 물류 견적 자동화 **우수상** |
| 2024.10 | 삼성전자·생기연 연계 프로젝트 — AI 오토 라벨러 **우수상** |
| 2024.05 | 금융 비교 플랫폼 'FINFO' — **대상** |
| 2023.12 | **빅데이터분석기사** · 2022 **ADsP** 자격 |

📄 연구: [아파트 가격 예측(CNN)](https://www.dbpia.co.kr/journal/articleDetail?nodeId=NODE11784072) · [뮤직카우 LSTM 예측](https://www.dbpia.co.kr/journal/articleDetail?nodeId=NODE11189989)

---

## 🔗 Links

## Recent Recognition

- **kr-bid** — 4th place, 2026 Public Procurement Data and AI Startup Contest, product and service development category.
- **SafetyEye** — Encouragement Award, 5th Employment and Labor Public Data and AI Utilization Contest, product and service development category.
- **AI Promptathon** — Award recipient, [WeMakeAppInToss](https://github.com/WeMakeAppInToss).

## Open Source

- [rxls](https://github.com/HyunjoJung/rxls) — native Rust spreadsheet library and CLI.
- [rwml](https://github.com/HyunjoJung/rwml) — native Rust Word reader/writer and PDF renderer.

[![Gmail](https://img.shields.io/badge/Gmail-j96263732@gmail.com-red?style=flat-square&logo=gmail&logoColor=white)](mailto:j96263732@gmail.com)
[![solved.ac](https://img.shields.io/badge/solved.ac-mrjung0987-0093FF?style=flat-square)](https://solved.ac/profile/mrjung0987)
