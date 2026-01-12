
export interface IUserInfo {
    name: string;
    description: string;
    avatarUrl: string;
}

export default interface IMediaSiteInfo {
    title: string;
    content: string;
    coverImageUrl: string;
    userInfo: IUserInfo;
}