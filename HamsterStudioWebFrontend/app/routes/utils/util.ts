
const backendUrl = import.meta.env.VITE_BACKEND_URL || 'http://localhost:5000';

export function getBackendUrl(): string {
    return backendUrl;
}

export function getProxyImageUrl(imageUrl: string): string {
    return `${backendUrl}/api/image-proxy?url=${encodeURIComponent(imageUrl)}`;
}

export function getCarouselImages(): string[] {
    const files = [
        "浴室拍照📷_1_xhs_.._1040g3k831pgspq1njk705num45u081vkrp77tig.png",
        "浴室拍照📷_2_xhs_.._1040g3k831pgspq1njk7g5num45u081vk8aoiljg.png",
        "浴室拍照📷_3_xhs_.._1040g3k831pgspq1njk805num45u081vkdgnup5o.png",
        "浴室拍照📷_4_xhs_.._1040g3k831pgspq1njk8g5num45u081vkil0lqr8.png",
        "浴室拍照📷_5_xhs_.._1040g3k831pgspq1njk905num45u081vks2th6no.png",
        "浴室拍照📷_6_xhs_.._1040g3k831pgspq1njk9g5num45u081vkfmiqsp8.png",
    ];
    return files.map(file => `${backendUrl}/static/${file}`);
}
