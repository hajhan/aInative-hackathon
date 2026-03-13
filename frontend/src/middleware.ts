import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// 인증이 필요한 경로 패턴
const protectedPaths = ["/home", "/medications", "/reports", "/history", "/ocr"];

// 인증된 사용자가 접근하면 안 되는 경로
const authPaths = ["/login", "/register"];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // sessionStorage는 서버 측 미들웨어에서 접근 불가
  // 쿠키 기반 인증 플래그 확인 (httpOnly가 아닌 일반 쿠키)
  const authCookie = request.cookies.get("sr_auth_flag");
  const isAuthenticated = !!authCookie?.value;

  // 루트 경로 처리
  if (pathname === "/") {
    if (isAuthenticated) {
      return NextResponse.redirect(new URL("/home", request.url));
    }
    return NextResponse.redirect(new URL("/login", request.url));
  }

  // 보호된 경로 — 미인증 시 로그인으로 redirect
  if (protectedPaths.some((path) => pathname.startsWith(path))) {
    if (!isAuthenticated) {
      const loginUrl = new URL("/login", request.url);
      loginUrl.searchParams.set("redirect", pathname);
      return NextResponse.redirect(loginUrl);
    }
  }

  // 인증 경로 — 이미 인증된 경우 홈으로 redirect
  if (authPaths.some((path) => pathname.startsWith(path))) {
    if (isAuthenticated) {
      return NextResponse.redirect(new URL("/home", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * 아래 경로를 제외한 모든 요청에 미들웨어 적용:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization)
     * - favicon.ico, manifest.json, icons, sw.js
     */
    "/((?!api|_next/static|_next/image|favicon.ico|manifest.json|icons|sw.js|workbox-).*)",
  ],
};
