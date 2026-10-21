using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("KSP 双 DLC 简体中文补丁安装程序")]
[assembly: AssemblyProduct("KSP DLC Chinese Patch")]
[assembly: AssemblyDescription("安装/卸载 创造历史 与 破土动工 的简体中文词典")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace KspDlcCn
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            string silentDir = null;
            string removeDir = null;
            foreach (string a in args)
            {
                if (a.StartsWith("/silent:", StringComparison.OrdinalIgnoreCase))
                    silentDir = a.Substring(8).Trim('"');
                else if (a.StartsWith("/remove:", StringComparison.OrdinalIgnoreCase))
                    removeDir = a.Substring(8).Trim('"');
            }

            if (removeDir != null)
            {
                string rr = Installer.Remove(removeDir);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "install-log.txt"),
                    rr, Encoding.UTF8);
                return rr.EndsWith("OK") ? 0 : 1;
            }

            if (silentDir != null)
            {
                var r = Installer.Install(silentDir);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "install-log.txt"),
                    r, Encoding.UTF8);
                return r.EndsWith("OK") ? 0 : 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }

    public class MainForm : Form
    {
        readonly TextBox _path = new TextBox();
        readonly TextBox _log = new TextBox();
        readonly Label _status = new Label();
        readonly Button _install = new Button();
        readonly Button _remove = new Button();
        readonly Button _browse = new Button();
        readonly Button _close = new Button();

        public MainForm()
        {
            Text = "KSP 双 DLC 简体中文补丁 v1.0";
            ClientSize = new Size(660, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Microsoft YaHei UI", 9f);

            var title = new Label();
            title.Text = "创造历史 与 破土动工 —— 简体中文词典";
            title.Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold);
            title.SetBounds(16, 12, 620, 26);
            Controls.Add(title);

            var tip = new Label();
            tip.Text = "本程序只补充两个 DLC 的中文，不覆盖任何原有文件；卸载即删除对应词典。";
            tip.ForeColor = Color.DimGray;
            tip.SetBounds(16, 42, 620, 20);
            Controls.Add(tip);

            var lbl = new Label();
            lbl.Text = "游戏目录：";
            lbl.SetBounds(16, 76, 80, 22);
            Controls.Add(lbl);

            _path.SetBounds(96, 74, 450, 24);
            Controls.Add(_path);

            _browse.Text = "浏览…";
            _browse.SetBounds(556, 73, 88, 26);
            _browse.Click += (s, e) => Browse();
            Controls.Add(_browse);

            _status.SetBounds(16, 106, 620, 40);
            _status.ForeColor = Color.DarkGreen;
            Controls.Add(_status);

            _install.Text = "安装中文";
            _install.SetBounds(16, 152, 150, 34);
            _install.Click += (s, e) => DoInstall();
            Controls.Add(_install);

            _remove.Text = "卸载（还原英文）";
            _remove.SetBounds(178, 152, 170, 34);
            _remove.Click += (s, e) => DoRemove();
            Controls.Add(_remove);

            _close.Text = "退出";
            _close.SetBounds(556, 152, 88, 34);
            _close.Click += (s, e) => Close();
            Controls.Add(_close);

            _log.SetBounds(16, 198, 628, 216);
            _log.Multiline = true;
            _log.ReadOnly = true;
            _log.ScrollBars = ScrollBars.Vertical;
            _log.BackColor = Color.White;
            Controls.Add(_log);

            string found = Installer.FindKsp();
            if (found != null) { _path.Text = found; }
            UpdateStatus();
            _path.TextChanged += (s, e) => UpdateStatus();
        }

        void Browse()
        {
            using (var d = new FolderBrowserDialog())
            {
                d.Description = "选择 Kerbal Space Program 文件夹（里面应有 GameData 文件夹）";
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _path.Text = d.SelectedPath;
                    UpdateStatus();
                }
            }
        }

        void UpdateStatus()
        {
            string p = _path.Text.Trim();
            if (p.Length == 0)
            {
                _status.ForeColor = Color.DarkOrange;
                _status.Text = "未自动找到游戏，请点“浏览…”选择 KSP 安装目录。";
                return;
            }
            if (!Installer.IsKspDir(p))
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "这个目录看起来不是 KSP（缺少 GameData\\Squad）。";
                return;
            }
            _status.ForeColor = Color.DarkGreen;
            _status.Text = Installer.HasBaseChinese(p)
                ? "已检测到 KSP，且本体已有中文词典。可以直接安装。"
                : "已检测到 KSP，但本体还没有中文：本补丁只翻 DLC，请先装本体汉化（如 TinyGrox 的 KSP_Chinese_Patches）。";
            if (!Installer.HasBaseChinese(p)) _status.ForeColor = Color.DarkOrange;
        }

        void Log(string s)
        {
            _log.AppendText(s + Environment.NewLine);
        }

        void DoInstall()
        {
            string p = _path.Text.Trim();
            if (!Installer.IsKspDir(p)) { MessageBox.Show(this, "请先选择正确的 KSP 文件夹。", Text); return; }
            if (Installer.IsGameRunning())
            {
                MessageBox.Show(this, "检测到 KSP 正在运行，请先完全关闭游戏再安装。", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _install.Enabled = false;
            try
            {
                _log.Clear();
                string report = Installer.Install(p);
                foreach (string line in report.Split('\n')) Log(line.TrimEnd('\r'));
            }
            finally { _install.Enabled = true; }
            UpdateStatus();
        }

        void DoRemove()
        {
            string p = _path.Text.Trim();
            if (!Installer.IsKspDir(p)) { MessageBox.Show(this, "请先选择正确的 KSP 文件夹。", Text); return; }
            if (MessageBox.Show(this, "将删除两个 DLC 的中文词典文件（并尝试恢复安装前的 settings.cfg），确定吗？",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _log.Clear();
            foreach (string line in Installer.Remove(p).Split('\n')) Log(line.TrimEnd('\r'));
            UpdateStatus();
        }
    }

    static class Installer
    {
        public static readonly string[] DlcDirs = { "MakingHistory", "Serenity" };

        public static bool IsKspDir(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return false;
            try { return Directory.Exists(Path.Combine(dir, "GameData", "Squad")); }
            catch { return false; }
        }

        public static bool IsGameRunning()
        {
            return Process.GetProcessesByName("KSP_x64").Length > 0 || Process.GetProcessesByName("KSP").Length > 0;
        }

        public static bool HasBaseChinese(string ksp)
        {
            try
            {
                string f = Path.Combine(ksp, "GameData", "Squad", "Localization", "dictionary.cfg");
                if (!File.Exists(f)) return false;
                return File.ReadAllText(f, Encoding.UTF8).Contains("zh-cn");
            }
            catch { return false; }
        }

        static string Reg(RegistryKey root, string sub, string name)
        {
            try
            {
                using (RegistryKey k = root.OpenSubKey(sub))
                {
                    if (k == null) return null;
                    object v = k.GetValue(name);
                    return v == null ? null : v.ToString();
                }
            }
            catch { return null; }
        }

        public static string FindKsp()
        {
            var libs = new List<string>();

            string steam = Reg(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");
            if (steam == null) steam = Reg(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath");
            if (steam == null) steam = Reg(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath");
            if (steam != null) libs.Add(steam);

            foreach (string lib in new List<string>(libs))
            {
                string vdf = Path.Combine(lib, @"steamapps\libraryfolders.vdf");
                try
                {
                    if (!File.Exists(vdf)) continue;
                    foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s*\"([^\"]+)\""))
                        libs.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
                }
                catch { }
            }

            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                try
                {
                    if (d.DriveType != DriveType.Fixed || !d.IsReady) continue;
                    libs.Add(Path.Combine(d.RootDirectory.FullName, "SteamLibrary"));
                }
                catch { }
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string lib in libs)
            {
                if (string.IsNullOrEmpty(lib)) continue;
                string p = Path.Combine(lib.TrimEnd('\\'), @"steamapps\common\Kerbal Space Program");
                if (!seen.Add(p)) continue;
                if (IsKspDir(p)) return p;
            }
            return null;
        }

        static byte[] Resource(string name)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (s == null) throw new Exception("内置词典缺失：" + name);
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static string Install(string ksp)
        {
            var sb = new StringBuilder();
            try
            {
                if (!IsKspDir(ksp)) return "ERR: 目标目录不是 KSP：" + ksp;
                sb.AppendLine("目标：" + ksp);
                if (!Directory.Exists(Path.Combine(ksp, "GameData", "SquadExpansion")))
                    sb.AppendLine("警告：没找到 GameData\\SquadExpansion，你可能没装这两个 DLC。");

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string settings = Path.Combine(ksp, "settings.cfg");
                bool hasBackup = Directory.Exists(ksp) && Directory.GetFiles(ksp, "settings.cfg.bak_*").Length > 0;
                if (File.Exists(settings) && !hasBackup)
                {
                    File.Copy(settings, settings + ".bak_" + stamp, true);
                    sb.AppendLine("已备份 settings.cfg -> settings.cfg.bak_" + stamp);
                }

                foreach (string dlc in DlcDirs)
                {
                    string dir = Path.Combine(ksp, "GameData", "SquadExpansion", dlc, "Localization");
                    Directory.CreateDirectory(dir);
                    byte[] data = Resource(dlc == "Serenity" ? "KspDlcCn.serenity.cfg" : "KspDlcCn.makinghistory.cfg");
                    File.WriteAllBytes(Path.Combine(dir, "zh-cn.cfg"), data);
                    sb.AppendLine("已写入 " + dlc + @"\Localization\zh-cn.cfg（" + data.Length + " 字节）");
                }

                if (File.Exists(settings))
                {
                    string text = File.ReadAllText(settings, Encoding.UTF8);
                    if (Regex.IsMatch(text, @"(?m)^\s*LANGUAGE\s*="))
                        text = Regex.Replace(text, @"(?m)^\s*LANGUAGE\s*=.*$", "LANGUAGE = zh-cn");
                    else
                        text = "LANGUAGE = zh-cn" + Environment.NewLine + text;
                    File.WriteAllText(settings, text, new UTF8Encoding(false));
                    sb.AppendLine("已把 settings.cfg 的 LANGUAGE 设为 zh-cn");
                }
                else
                {
                    File.WriteAllText(settings, "SETTINGS_FILE_VERSION = 1.3.0\r\nLANGUAGE = zh-cn\r\n", new UTF8Encoding(false));
                    sb.AppendLine("未找到 settings.cfg，已新建一个把语言设为 zh-cn（进游戏后请在设置里再确认一次）");
                }

                if (!HasBaseChinese(ksp))
                    sb.AppendLine("提示：本体尚未检测到中文词典，本体界面可能仍是英文。请另行安装本体汉化包。");

                sb.AppendLine("完成。启动游戏，设置 -> 综合 -> 语言 应为“简体中文”。");
                sb.Append("OK");
            }
            catch (UnauthorizedAccessException ua)
            {
                sb.AppendLine("ERR: 没有写入权限（" + ua.Message + "）。");
                sb.AppendLine("请右键点击本程序，选择“以管理员身份运行”后重试。");
            }
            catch (Exception ex)
            {
                sb.AppendLine("ERR: " + ex.Message);
            }
            return sb.ToString();
        }

        public static string Remove(string ksp)
        {
            var sb = new StringBuilder();
            try
            {
                foreach (string dlc in DlcDirs)
                {
                    string f = Path.Combine(ksp, "GameData", "SquadExpansion", dlc, "Localization", "zh-cn.cfg");
                    if (File.Exists(f)) { File.Delete(f); sb.AppendLine("已删除 " + f); }
                }

                string settings = Path.Combine(ksp, "settings.cfg");
                string dir = Path.GetDirectoryName(settings);
                string[] baks = Directory.GetFiles(dir, "settings.cfg.bak_*");
                if (baks.Length > 0)
                {
                    Array.Sort(baks, StringComparer.OrdinalIgnoreCase);
                    string latest = baks[baks.Length - 1];
                    File.Copy(latest, settings, true);
                    sb.AppendLine("已恢复 " + Path.GetFileName(latest));
                }
                else if (File.Exists(settings))
                {
                    string text = File.ReadAllText(settings, Encoding.UTF8);
                    text = Regex.Replace(text, @"(?m)^\s*LANGUAGE\s*=.*$", "LANGUAGE = en-us");
                    File.WriteAllText(settings, text, new UTF8Encoding(false));
                    sb.AppendLine("无备份可用，已把 LANGUAGE 改回 en-us");
                }
                sb.AppendLine("完成。");
                sb.Append("OK");
            }
            catch (Exception ex)
            {
                sb.AppendLine("ERR: " + ex.Message);
            }
            return sb.ToString();
        }
    }
}
