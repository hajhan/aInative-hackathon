"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

interface NavItem {
  href: string;
  label: string;
  icon: string;
}

const navItems: NavItem[] = [
  { href: "/home", label: "홈", icon: "🏠" },
  { href: "/medications", label: "복용약", icon: "💊" },
  { href: "/reports/new", label: "보고", icon: "📋" },
  { href: "/history", label: "이력", icon: "📅" },
];

export function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 z-40 safe-area-bottom">
      <div className="max-w-md mx-auto">
        <ul className="flex items-stretch">
          {navItems.map((item) => {
            const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
            return (
              <li key={item.href} className="flex-1">
                <Link
                  href={item.href}
                  className={`flex flex-col items-center justify-center py-2 min-h-touch w-full no-underline transition-colors ${
                    isActive
                      ? "text-primary-600"
                      : "text-gray-500 hover:text-primary-500"
                  }`}
                >
                  <span className="text-xl leading-none mb-0.5">{item.icon}</span>
                  <span className="text-xs font-medium">{item.label}</span>
                </Link>
              </li>
            );
          })}
        </ul>
      </div>
    </nav>
  );
}
