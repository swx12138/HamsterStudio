import "./home.css";

import { useState } from "react";
import type { Route } from "./+types/home";
import { Image, Layout, theme, Splitter, Card, Tooltip, Carousel, Flex } from 'antd';
import type IMediaSiteInfo from "./interfaces/IMediaSiteInfo";
import { getCarouselImages, getProxyImageUrl } from "./utils/util";
import { Desc } from "./components/desc";
import type { IUserInfo } from "./interfaces/IMediaSiteInfo";
import UserInfo from "./components/userinfo";

const { Header, Footer, Sider, Content } = Layout;
const { Meta } = Card;

export function meta({ }: Route.MetaArgs) {
    return [
        { title: "Hamster Studio" },
        { name: "description", content: "Welcome to React Router!" },
    ];
}

const contentStyle: React.CSSProperties = {
    margin: 0,
    height: '160px',
    color: '#fff',
    lineHeight: '160px',
    textAlign: 'center',
    background: '#364d79',
};

export default function Home() {
    const [mediaSiteInfo, setMediaSiteInfo] = useState<IMediaSiteInfo>(new class implements IMediaSiteInfo {
        title: string = "Title";
        content: string = "https://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpg+https://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpg+https://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpg+https://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpghttps://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpghttps://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpg";
        coverImageUrl: string = "https://i0.hdslb.com/bfs/archive/b94dee111982fe0071732a36c8b6096f24ac2a75.jpg";
        userInfo = new class implements IUserInfo {
            name = "Username";
            description = "拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。拉屎是起床的唯一驱动力。";
            avatarUrl = "https://i0.hdslb.com/bfs/face/da2336247354673c14adb3a69226cdf9bcead5ca.jpg@240w_240h_1c_1s_!web-avatar-nav.avif"
        };
    });
    const {
        token: { colorBgContainer, borderRadiusLG },
    } = theme.useToken();
    return (<>
        <Flex vertical>
            <Carousel
                className="top-carousel"
                lazyLoad={"progressive"}
                centerMode={true} autoplay>
                {getCarouselImages().map((url, index) => (
                    <Image
                        rootClassName="slick-track-image-container"
                        className="contain-fit-image"
                        src={url} />
                ))}
            </Carousel>
            <Card
                className="main-card"
                title={mediaSiteInfo.title}
                cover={<Image
                    rootClassName="slick-track-image-container"
                    className="contain-fit-image"
                    src={getProxyImageUrl(mediaSiteInfo.coverImageUrl)} />} >
                <UserInfo info={mediaSiteInfo.userInfo} />
            </Card>
        </Flex>
    </>);
}
