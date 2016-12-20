using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Management;

namespace WallPaperGuy
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        //retrieve the file path of the currently set wallpaper (only supports windows 7, 8, 10 & maybe XP & vista although it's untested)
        internal static string GetWallpaperPath()
        {
            try
            {
                string wallpaperFile;

                switch (GetWindowsVersionNumber())
                {
                    case null:
                        return null;
                    case "XP": //windows XP (untested)
                        var xpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
                        if (xpReg == null) return null;

                        wallpaperFile = xpReg.GetValue("WallpaperSource").ToString();
                        xpReg.Close();
                        break;
                    case "Vista": //windows vista (untested)
                        var vistaReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
                        if (vistaReg == null) return null;

                        wallpaperFile = vistaReg.GetValue("WallpaperSource").ToString();
                        vistaReg.Close();
                        break;
                    case "7": //windows 7
                        var w7Reg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
                        if (w7Reg == null) return null;

                        wallpaperFile = w7Reg.GetValue("WallpaperSource").ToString();
                        w7Reg.Close();
                        break;
                    default: //all other windows, but assumed 8/10
                        Console.WriteLine("8/10");
                        var openSubKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop");
                        if (openSubKey == null) return null;
                        
                        var path = (byte[])openSubKey.GetValue("TranscodedImageCache");
                        wallpaperFile = Encoding.Unicode.GetString(Chop(path, 24)).TrimEnd("\0".ToCharArray());
                        break;
                }

                return wallpaperFile;
            }
            catch (Exception) { return null; }
        }

        //return windows version (ie. XP, Vista, 7, 8, 10)
        internal static string GetWindowsVersionNumber()
        {
            var osName = (from cap in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                select cap.GetPropertyValue("Caption")).FirstOrDefault();

            if (osName == null) return null;
            var infoArray = osName.ToString().Split(' ');
            
            return infoArray[2];
        }

        static byte[] Chop(byte[] source, int pos)
        {
            var dest = new byte[source.Length - pos];
            Array.Copy(source, pos, dest, 0, dest.Length);

            return dest;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            wallpaperTimer.Enabled = true; //start wallpaper timer
        }

        private void wallpaperTimer_Tick(object sender, EventArgs e)
        {
            //get wallpaper path and filename
            var path = GetWallpaperPath();

            //check if wallpaper is removed (or set or solid color) during operation and reset controls
            if (path == null || path.Trim() == "")
            {
                txtPath.Text = "";
                pbPreview.Image = null;
                return;
            }

            //set path text
            txtPath.Text = path;

            //load preview
            if (txtPath.Text != "" && File.Exists(txtPath.Text))
                pbPreview.Load(txtPath.Text);
        }

        //open containing folder of image
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Path.GetDirectoryName(txtPath.Text);

                Process.Start("explorer.exe", @path);
            }
            catch (Exception) { MessageBox.Show("Error: Path doesn't exist!", "WallPaperGuy by Kez", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        //delete current wallpaper (might be quirky on windows 10)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var fileName = Path.GetFileName(txtPath.Text);

                if (fileName == null || fileName.Trim() == "") return;

                var answer = MessageBox.Show("Are you sure you want to delete file: [" + fileName + "]?", "WallPaperGuy by Kez", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes) return;
                
                File.Delete(txtPath.Text);
            }
            catch (Exception) { MessageBox.Show("Error!", "WallPaperGuy by Kez", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
