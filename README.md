# SeatKiller_UI

[![License](https://img.shields.io/badge/License-MPL--2.0-red.svg)](LICENSE)
[![Language](https://img.shields.io/badge/language-C%23-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
![Github All Releases](https://img.shields.io/github/downloads/goolhanrry/Seatkiller_UI/total.svg)
[![GitHub repo size in bytes](https://img.shields.io/github/repo-size/goolhanrry/SeatKiller_UI.svg?colorB=ff7e00#)](https://github.com/goolhanrry/SeatKiller_UI)

用C#重写的 [SeatKiller](https://github.com/goolhanrry/SeatKiller) GUI 版本，新增了座位改签和座位锁定的功能，欢迎添加我的微信：`aweawds` 交流讨论

代码引用请遵守 `Mozilla Public License 2.0` 协议，严禁修改代码后闭源，如有违约后果自负

不想折腾的话可以 [点击此处](https://github.com/goolhanrry/SeatKiller_UI/releases) 下载编译好的最新版本的软件

## 已经实现的功能

* 获取用户的信息，包括姓名、上次入馆时间、当前状态（未入馆、已进入某分馆）和累计违约次数
* 每次启动时自动检查更新
* 晚上22:15定时抢次日座位（可自行选择是否指定区域、座位号）
* 检测是否已有有效预约，区分状态（预约、履约中、暂离），并可取消或释放座位
* 显示和指定座位类别，包括电源、靠窗、电脑
* 预约成功后发送邮件提醒
* 捡漏模式可用于抢当天座位
* 改签模式可用于在已有有效预约的情况下，更换座位或改签预约时间（如果空闲）
* 座位处于预约或暂离状态时，在预约信息即将失效前自动释放座位并重新预约，防止座位丢失
* 循环抢座功能可用于每天自动抢上一次的座位
* 采用256位AES算法加密学号和密码并写入注册表，提高登录效率（此处涉及到注册表操作，系统或杀毒软件可能会报毒，可以放心允许操作）

## 即将实现的功能

* 暂无

## 软件截图

<p align="center">
  <img with="340" height="280" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot1.png" alt="screenshot1">
</p>
<p align="center">
  <img with="376" height="250" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot2.png" alt="screenshot4">
</p>
<p align="center">
  <img with="598" height="421" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot3.png" alt="screenshot2">
</p>
<p align="center">
  <img with="598" height="421" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot4.png" alt="screenshot3">
</p>
<p align="center">
  <img with="376" height="250" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot5.png" alt="screenshot4">
</p>
<p align="center">
  <img with="598" height="421" src="https://github.com/goolhanrry/SeatKiller_UI/blob/master/Screenshots/SeatKiller_UI_Screenshot6.png" alt="screenshot5">
</p>
