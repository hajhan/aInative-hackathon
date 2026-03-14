import { Header } from "@/components/layout/Header";
import { BottomNav } from "@/components/layout/BottomNav";
import { AuthGuard } from "@/components/auth/AuthGuard";

/**
 * 인증된 사용자용 메인 레이아웃
 * Header + BottomNav 포함
 */
export default function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <div className="min-h-screen bg-gray-50 flex flex-col">
        <Header />
        <main className="flex-1 pb-20">{children}</main>
        <BottomNav />
      </div>
    </AuthGuard>
  );
}
