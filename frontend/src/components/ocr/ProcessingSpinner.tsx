export default function ProcessingSpinner() {
  return (
    <div className="flex flex-col items-center justify-center py-16 space-y-4">
      <div className="relative w-16 h-16">
        <div className="absolute inset-0 rounded-full border-4 border-blue-100" />
        <div className="absolute inset-0 rounded-full border-4 border-blue-500 border-t-transparent animate-spin" />
      </div>
      <p className="text-lg font-medium text-gray-700">약봉투를 인식하고 있어요...</p>
      <p className="text-sm text-gray-500">잠시만 기다려 주세요 (2~4초)</p>
    </div>
  );
}
