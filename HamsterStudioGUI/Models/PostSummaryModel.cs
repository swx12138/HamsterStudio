﻿namespace HamsterStudioGUI.Models;

internal record PostSummaryModel(
    string Like = "0",
    string Reply = "0",
    string Favorite = "0",
    string View = "0",
    string Share = "0",
    string Coin = "0",
    string Danmaku = "0")
{ }