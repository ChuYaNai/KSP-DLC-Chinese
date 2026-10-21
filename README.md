# KSP 双 DLC 简体中文补丁

《坎巴拉太空计划》**创造历史 (Making History)** 与 **破土动工 (Breaking Ground)** 的简体中文词典补丁。

游戏本体自带的中文（或社区本体汉化）并不包含这两个 DLC 的文本：`Making History` 目录下甚至没有 `Localization` 文件夹，`Serenity`（破土动工）只带了一份 731 行的 `en-us` 词典。结果是本体全中文、DLC 部件和界面全英文。

本补丁补上这部分，约 **1000 条**词条，不改动任何原有文件。

## 覆盖内容

| DLC | 覆盖 |
|---|---|
| 创造历史 Making History | 全部部件名称与描述、任务编辑器节点（节点名/分类/说明）、旗帜与部件变体 |
| 破土动工 Breaking Ground | 机器人部件（铰链/伺服/活塞/转子/螺旋桨）、部署科学全套实验与界面、地表特征与扫描机械臂、KAL-1000 轨道编辑器、科研结果文案 |

## 安装

### 方式一：安装程序（推荐）

下载 [Releases](../../releases) 里的 `KSP_DLC_Chinese_Patch_v1.0.exe`，双击：

1. 关闭游戏（安装器会检测）；
2. 程序自动查找 KSP 目录（Steam 注册表 + 全部 Steam 库 + 各盘 `SteamLibrary`），找不到可手动浏览；
3. 点「安装中文」，自动备份 `settings.cfg` 并把语言设为 `zh-cn`；
4. 启动游戏，确认 `设置 → 综合 → 语言 = 简体中文`。

卸载：打开程序点「卸载（还原英文）」，会删除词典并恢复安装前的 `settings.cfg`。

### 方式二：手动

把本仓库的 `GameData` 文件夹内容按相同结构复制进 KSP 根目录：

```
GameData\SquadExpansion\MakingHistory\Localization\zh-cn.cfg
GameData\SquadExpansion\Serenity\Localization\zh-cn.cfg
```

再把 `settings.cfg` 里的 `LANGUAGE` 改为 `zh-cn`，重启游戏。

## 重要说明

- **本补丁只翻 DLC，不含游戏本体中文。** 本体仍是英文的话，请先安装本体汉化
  （例如 [TinyGrox/KSP_Chinese_Patches](https://github.com/tinygrox/KSP_Chinese_Patches)）。
- **创造历史的任务剧情正文翻不了。** 简报、对白写在 `Missions\*.mission` 文件里，
  是纯文本资源，不走 `#autoLOC` 词典机制，因此保持英文。
- 音译/梗名（如“乳齿象”“科迪亚克棕熊”“快艇”）参考了社区常用译法，欢迎 PR 修正。
- 官方源里不少 MH 描述本身就是占位符 `Description Creation Pending`，此处照实译为「描述待编写」。

## 从源码构建

需要 Windows + .NET Framework 4.x 的 `csc.exe`（Win10/11 自带）。

```powershell
.\src\build.ps1
```

产物在 `dist\`。脚本会用 `/resource:` 把两个 cfg 内嵌进单一 exe。

## 目录结构

```
GameData/SquadExpansion/...      词典文件（手动安装用）
src/Installer.cs                 安装器源码（WinForms，单文件）
src/build.ps1                    编译脚本
dist/                            编译产物（exe + 说明）
```

## 免责声明

非官方粉丝汉化，与 Squad / Private Division / Take-Two 无关。
《Kerbal Space Program》及其 DLC 的文本与素材版权归其各自权利人所有。
本仓库仅包含社区翻译文本与安装脚本，仅供个人学习交流使用，请勿用于商业用途。

## License

- 代码（`src/`）：MIT
- 翻译文本（`GameData/`）：CC BY-NC-SA 4.0
