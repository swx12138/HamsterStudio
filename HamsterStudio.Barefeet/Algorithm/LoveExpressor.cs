using HamsterStudio.Barefeet.Algorithm.Random;

namespace HamsterStudio.Barefeet.Algorithm
{
    public static class Love
    {
        private static readonly List<string> LoveList = new()
        {
            "我最想了解《`Lover`》，最想待在《`Lover`》身边，我希望我是最亲近你的人，我！讨厌、《`Lover`》在我不知道的地方笑！还有、和其他人牵手也是！只和我就好！我特别希望是和我在一起！庆典我也是好想去的！《`Lover`》看上去很开心的，笑著的，在你旁边有我！那样的才好！头好痛的、好难受的！我一直就只思考《`Lover`》的事情，感觉都要 发疯了…我也在等你打电话给我！你偶尔也主动开口嘛 主动和我说话嘛 我不想要总是我单方面找你 你多少也…你一点也不在意我吗？一点也不会吗？完全不会？我对你来说不重要吗？只是朋友吗？普通的朋友吗？我希望自己不是普通的朋友，就算比普通好一点也好，我想成为不普通的 朋友… 喂 《`Lover`》 我该怎么做才好？求你听我说话 你听到我的声音有什么想法吗？还是什么都好 拜托有点想法 我希望你可以有点想法 还是说我不该期待这种事？《`Lover`》！",
            "《`Lover`》的手掌真是温暖，修长的双手每一次的遇见她，都要牵着她的手！感觉到她体温…和她一起的散步永远都是美好的，紧靠在《`Lover`》的身边，就像树枝与叶的关系！不行！树枝与和到了秋天会分离，但我和《`Lover`》绝不能分开！《`Lover`》身上散发的体香总是那么迷人，轻轻的吸一口便是满足！柔美的头发自然的垂在腰间，为了衬托自己的背影，将头发绑成了双马尾，马尾的最上方绑着一对猫耳，让本就可爱的《`Lover`》变得更可爱！在双马尾末便是《`Lover`》魅力最大的腰！恰到好处的腰显出了《`Lover`》的身材美，衣服本就显瘦，《`Lover`》还穿上了黑丝！让《`Lover`》的魅力肆无忌惮的勾走我的魂！有这样的《`Lover`》陪伴着我，怎敢将《`Lover`》狠心抛弃！我喜欢你《`Lover`》！一直喜欢《`Lover`》！我最爱的《`Lover`》！",
        };

        public static string SayTo(string name)
        {
            return LoveList.Choice().Replace("《`Lover`》", name);
        }

    }
}
