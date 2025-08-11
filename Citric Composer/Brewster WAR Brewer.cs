using CitraFileLoader;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Citric_Composer {
    public partial class Brewster_WAR_Brewer : EditorBase {

        WaveOut waveOut = new WaveOut();
        bool paused;
        int prevIndex;
        byte forceWavMaj = 1;
        byte forceWavMin = 0;
        byte forceWavRev = 0;
        bool playNext;
        bool stopped = true;
        bool kill;

        public Brewster_WAR_Brewer(MainWindow mainWindow) : base(typeof(SoundWaveArchive), "波形档案", "war", "布鲁斯特档案归档大师", mainWindow) {
            InitializeComponent();
            Text = "布鲁斯特档案归档大师";
            Icon = Properties.Resources.Brewster;
            toolsWarToolStripMenuItem.Visible = true;
            waveOut.Stopped += new EventHandler<PlaybackStoppedEventArgs>(this.WaveEnded);
            new Thread(PlayNext).Start();
        }

        public Brewster_WAR_Brewer(string fileToOpen, MainWindow mainWindow) : base(typeof(SoundWaveArchive), "波形档案", "war", "布鲁斯特档案归档大师", fileToOpen, mainWindow) {
            InitializeComponent();
            Text = "布鲁斯特档案归档大师-" + Path.GetFileName(fileToOpen);
            Icon = Properties.Resources.Brewster;
            toolsWarToolStripMenuItem.Visible = true;
            waveOut.Stopped += new EventHandler<PlaybackStoppedEventArgs>(this.WaveEnded);
            new Thread(PlayNext).Start();
        }

        public Brewster_WAR_Brewer(SoundFile<ISoundFile> fileToOpen, MainWindow mainWindow, EditorBase otherEditor = null) : base(typeof(SoundWaveArchive), "波形档案", "war", "布鲁斯特档案归档大师", fileToOpen, mainWindow) {
            InitializeComponent();
            string name = ExtFile.FileName;
            if (name == null) {
                name = "{空的文件名称}";
            }
            Text = EditorName + " - " + name + "." + ExtFile.FileExtension;
            Icon = Properties.Resources.Brewster;
            OtherEditor = otherEditor;
            toolsWarToolStripMenuItem.Visible = true;
            waveOut.Stopped += new EventHandler<PlaybackStoppedEventArgs>(this.WaveEnded);
            new Thread(PlayNext).Start();
        }


        //Info and updates.
        #region InfoAndUpdates

        /// <summary>
        /// Do info stuff.
        /// </summary>
        public override void DoInfoStuff() {

            //Call base.
            base.DoInfoStuff();

            //Safety check.
            if (!FileOpen || File == null) {
                return;
            }

            //Parent is not null, so a wave is selected.
            if (tree.SelectedNode.Parent != null) {

                //File is null.
                if ((File as SoundWaveArchive)[tree.SelectedNode.Index] == null) {

                    //Null info.
                    nullDataPanel.BringToFront();
                    nullDataPanel.Show();

                    //Update status.
                    status.Text = "未选择有效信息!";

                }

                //File is valid.
                else {

                    //Sound player deluxe.
                    soundPlayerDeluxePanel.BringToFront();
                    soundPlayerDeluxePanel.Show();

                    //Update status.
                    status.Text = "波形: " + tree.SelectedNode.Index + ", 大小: " + (File as SoundWaveArchive)[tree.SelectedNode.Index].Wav.fileHeader.size + "字节.";

                }

            }

            //Waves list is selected.
            else if (tree.SelectedNode.Index == 1) {

                //Show no info screen.
                noInfoPanel.BringToFront();
                noInfoPanel.Show();

                //Show the status.
                status.Text = "波形数量: " + (File as SoundWaveArchive).Count;

            }

            //File information.
            else if (tree.SelectedNode.Index == 0) {

                //Proper info panel.
                warFileInfoPanel.BringToFront();
                warFileInfoPanel.Show();

                //Update boxes.
                vMajBoxWar.Value = (File as SoundWaveArchive).Version.Major;
                vMinBoxWar.Value = (File as SoundWaveArchive).Version.Minor;
                vRevBoxWar.Value = (File as SoundWaveArchive).Version.Revision;
                vWavMajBox.Value = forceWavMaj;
                vWavMinBox.Value = forceWavMin;
                vWavRevBox.Value = forceWavRev;

                //Status.
                status.Text = "文件信息.";

            }

        }

        /// <summary>
        /// Update nodes.
        /// </summary>
        public override void UpdateNodes() {

            //Begin update.
            BeginUpdateNodes();

            //Add waves if node doesn't exist.
            if (tree.Nodes.Count < 2) {
                tree.Nodes.Add("waves", "波形声音", 6, 6);
            }

            //File is open and not null.
            if (FileOpen && File != null) {

                //Get the version.
                for (int i = 0; i < (File as SoundWaveArchive).Count; i++) {

                    if ((File as SoundWaveArchive)[i] != null) {
                        forceWavMaj = (File as SoundWaveArchive)[i].Wav.fileHeader.vMajor;
                        forceWavMin = (File as SoundWaveArchive)[i].Wav.fileHeader.vMinor;
                        forceWavRev = (File as SoundWaveArchive)[i].Wav.fileHeader.vRevision;
                    }

                }

                //Context menu.
                tree.Nodes["waves"].ContextMenuStrip = rootMenu;

                //Add each wave.
                var h = File as SoundWaveArchive;
                for (int i = 0; i < (File as SoundWaveArchive).Count; i++) {

                    //File is null.
                    if ((File as SoundWaveArchive)[i] == null) {

                        //Add null wave.
                        tree.Nodes["waves"].Nodes.Add("wave" + i, "{空的波形" + i + " }", 0, 0);

                    }

                    //Valid info.
                    else {

                        //Get name.
                        string name = "{未知波形名称}";

                        /* OLD WAVE NAME FINDER.
                        
                        //Main window.
                        if (MainWindow != null) {

                            //File is open.
                            if (MainWindow.File != null) {

                                List<Tuple<WaveArchivePair, int>> pairs = new List<Tuple<WaveArchivePair, int>>();
                                int wsdNum = 0;
                                foreach (var wsd in MainWindow.File.WaveSoundDatas) {
                                    foreach (var pair in (wsd.File.File as WaveSoundData).Waves) {
                                        pairs.Add(new Tuple<WaveArchivePair, int>(pair, wsdNum));
                                    }
                                    wsdNum++;
                                }
                                List<int> possibleWarIndices = new List<int>();
                                for (int j = 0; j < MainWindow.File.WaveArchives.Count; j++) {
                                    if (MainWindow.File.WaveArchives[j].File.File == ExtFile.File) {
                                        possibleWarIndices.Add(j);
                                    }
                                }
                                var matches = pairs.Where(x => x.Item1.WaveIndex == i && possibleWarIndices.Contains(x.Item1.WarIndex)).ToList();
                                for (int j = 0; j < matches.Count; j++) {

                                    if (MainWindow.File.WaveSoundDatas[matches[j].Item2].WaveIndex == i) {
                                        name = MainWindow.File.WaveSoundDatas[matches[j].Item2].Name;
                                    }

                                }                    

                            }

                        }

                        */

                        //Get wave name.
                        if ((File as SoundWaveArchive)[i].Name != null) {
                            name = (File as SoundWaveArchive)[i].Name;
                        }

                        //Add each wave.
                        tree.Nodes["waves"].Nodes.Add("wave" + i, "[" + i + "] " + name, 2, 2);

                    }

                    //Add context menu.
                    tree.Nodes["waves"].Nodes["wave" + i].ContextMenuStrip = nodeMenu;

                }

            } else {

                //Remove context menu.
                tree.Nodes["waves"].ContextMenuStrip = null;

            }

            //End update.
            EndUpdateNodes();

        }

        /// <summary>
        /// Node is double clicked.
        /// </summary>
        public override void NodeMouseDoubleClick() {

            //Safety check.
            if (!FileOpen || File == null) {
                return;
            }

            //If parent exists, then it is a wave.
            if (tree.SelectedNode.Parent != null) {

                //Wave is not null.
                if ((File as SoundWaveArchive)[tree.SelectedNode.Index] != null) {

                    //Open the wave in Isabelle.
                    IsabelleSoundEditor e = new IsabelleSoundEditor(this, tree.SelectedNode.Index, "Wave " + tree.SelectedNode.Index);
                    e.Show();

                } else {

                    //Insult user.
                    MessageBox.Show("你无法打开空的波形文件!", "通知:");

                }

            }

        }

        #endregion


        //Player.
        #region Player

        /// <summary>
        /// Play sound player deluxe.
        /// </summary>
        public override void Play() {

            playNext = false;
            if (!paused || prevIndex != tree.SelectedNode.Index) {
                waveOut.Stop();
                var n = new WaveFileReader(new MemoryStream((File as SoundWaveArchive)[tree.SelectedNode.Index].Riff.ToBytes()));
                waveOut.Initialize(n);
                prevIndex = tree.SelectedNode.Index;
            }
            waveOut.Play();
            stopped = false;
            tree.Select();
            paused = false;

        }

        /// <summary>
        /// Play the next wave.
        /// </summary>
        public void PlayNext() {

            while (!kill) {

                if (soundPlayerDeluxePlayOnceBox.Checked) {
                    stopped = true;
                }

                if (!paused && playNext && !stopped) {

                    //Next index.
                    try {
                        tree.Select();
                    } catch { }
                    if (soundPlayerDeluxePlayNextBox.Checked) {
                        prevIndex++;
                        try {
                            tree.SelectedNode = tree.Nodes[1].Nodes[prevIndex];
                            tree.SelectedNode.EnsureVisible();
                        } catch { }
                        DoInfoStuff();
                    }
                    playNext = false;

                    if (prevIndex < (File as SoundWaveArchive).Count) {
                        if ((File as SoundWaveArchive)[prevIndex] != null) {
                            var n = new WaveFileReader(new MemoryStream((File as SoundWaveArchive)[prevIndex].Riff.ToBytes()));
                            waveOut.Stop();
                            waveOut.Initialize(n);
                            waveOut.Play();
                        }
                    }

                }

                Thread.Sleep(100);

            }

        }

        /// <summary>
        /// Wave finished playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WaveEnded(object sender, PlaybackStoppedEventArgs e) {
            playNext = true;
        }

        /// <summary>
        /// Pause sound player deluxe.
        /// </summary>
        public override void Pause() {

            paused = true;
            waveOut.Pause();

        }

        /// <summary>
        /// Stop sound player deluxe.
        /// </summary>
        public override void Stop() {

            paused = false;
            waveOut.Stop();
            playNext = false;
            stopped = true;

        }

        #endregion


        //File info.
        #region FileInfo

        /// <summary>
        /// Force the internal waves to have the same version.
        /// </summary>
        public override void ForceWaveVersionButtonClick() {

            for (int i = 0; i < (File as SoundWaveArchive).Count; i++) {

                if ((File as SoundWaveArchive)[i] != null) {
                    (File as SoundWaveArchive)[i].Wav.fileHeader.vMajor = forceWavMaj;
                    (File as SoundWaveArchive)[i].Wav.fileHeader.vMinor = forceWavMin;
                    (File as SoundWaveArchive)[i].Wav.fileHeader.vRevision = forceWavRev;
                }

            }

        }

        /// <summary>
        /// WAR major.
        /// </summary>
        public override void BoxWarMajChanged() {
            (File as SoundWaveArchive).Version.Major = (byte)vMajBoxWar.Value;
        }

        /// <summary>
        /// WAR minor.
        /// </summary>
        public override void BoxWarMinChanged() {
            (File as SoundWaveArchive).Version.Minor = (byte)vMinBoxWar.Value;
        }

        /// <summary>
        /// WAR revision.
        /// </summary>
        public override void BoxWarRevChanged() {
            (File as SoundWaveArchive).Version.Revision = (byte)vRevBoxWar.Value;
        }

        /// <summary>
        /// WAV major.
        /// </summary>
        public override void BoxWavMajChanged() {
            forceWavMaj = (byte)vWavMajBox.Value;
        }

        /// <summary>
        /// WAV minor.
        /// </summary>
        public override void BoxWavMinChanged() {
            forceWavMin = (byte)vWavMinBox.Value;
        }

        /// <summary>
        /// WAV revision.
        /// </summary>
        public override void BoxWavRevChanged() {
            forceWavRev = (byte)vWavRevBox.Value;
        }

        #endregion


        //Node menus.
        #region NodeMenus

        /// <summary>
        /// Add a wave.
        /// </summary>
        public override void RootAdd() {
            Wave w = GetWave();
            if (w != null) {
                (File as SoundWaveArchive).Add(w);
                UpdateNodes();
            }
        }

        /// <summary>
        /// Add the node above.
        /// </summary>
        public override void NodeAddAbove() {
            Wave w = GetWave();
            if (w != null) {
                (File as SoundWaveArchive).Insert(tree.SelectedNode.Index, w);
                UpdateNodes();
                tree.SelectedNode = tree.Nodes["waves"].Nodes[tree.SelectedNode.Index + 1];
                DoInfoStuff();
            }
        }

        /// <summary>
        /// Add the node below.
        /// </summary>
        public override void NodeAddBelow() {
            Wave w = GetWave();
            if (w != null) {
                (File as SoundWaveArchive).Insert(tree.SelectedNode.Index + 1, w);
                UpdateNodes();
                DoInfoStuff();
            }
        }

        /// <summary>
        /// Move node up.
        /// </summary>
        public override void NodeMoveUp() {
            if (Swap(File as SoundWaveArchive, tree.SelectedNode.Index - 1, tree.SelectedNode.Index)) {
                UpdateNodes();
                tree.SelectedNode = tree.Nodes["waves"].Nodes[tree.SelectedNode.Index - 1];
                DoInfoStuff();
            }
        }

        /// <summary>
        /// Move node down.
        /// </summary>
        public override void NodeMoveDown() {
            if (Swap(File as SoundWaveArchive, tree.SelectedNode.Index + 1, tree.SelectedNode.Index)) {
                UpdateNodes();
                tree.SelectedNode = tree.Nodes["waves"].Nodes[tree.SelectedNode.Index + 1];
                DoInfoStuff();
            }
        }

        /// <summary>
        /// Blank a node.
        /// </summary>
        public override void NodeBlank() {
            NodeReplace();
        }

        /// <summary>
        /// Replace node.
        /// </summary>
        public override void NodeReplace() {
            Wave w = GetWave();
            if (w != null) {
                (File as SoundWaveArchive)[tree.SelectedNode.Index] = w;
                UpdateNodes();
                DoInfoStuff();
            }
        }

        /// <summary>
        /// Export node.
        /// </summary>
        public override void NodeExport() {
            Wave w = (File as SoundWaveArchive)[tree.SelectedNode.Index];
            if (w != null) {

                //Get export path.
                SaveFileDialog s = new SaveFileDialog();
                s.RestoreDirectory = true;
                s.FileName = tree.SelectedNode.Text;
                s.Filter = "波形|*.wav|波形(3ds或WiiU)|*.bfwav;*.bcwav|波形(Switch)|*.bfwav|流(3ds或WiiU)|*.bfstm;*.bcstm|流(Switch)|*.bfstm";
                s.ShowDialog();
                if (Path.GetExtension(s.FileName) == "") {
                    s.FileName += "wav";
                }
                if (s.FileName != "") {

                    //Get file data.
                    WriteMode m = WriteMode.Cafe;
                    if (Path.GetExtension(s.FileName).ToLower()[2] == 'c') {
                        m = WriteMode.CTR;
                    }
                    byte[] b = null;
                    switch (s.FilterIndex) {

                        //Wave.
                        case 1:
                            b = RiffWaveFactory.CreateRiffWave(w.Wav).ToBytes();
                            break;

                        //SDK Wave.
                        case 2:
                            if (m == WriteMode.Cafe) {
                                b = w.Wav.ToBytes(ByteOrder.BigEndian, true);
                            } else {
                                b = w.Wav.ToBytes(ByteOrder.LittleEndian);
                            }
                            break;

                        //SDK Wave Switch.
                        case 3:
                            b = w.Wav.ToBytes(ByteOrder.LittleEndian, true);
                            break;

                        //SDK Stream.
                        case 4:
                            if (m == WriteMode.Cafe) {
                                b = StreamFactory.CreateStream(w.Wav, w.Wav.fileHeader.vMajor, w.Wav.fileHeader.vMinor, w.Wav.fileHeader.vRevision).ToBytes(ByteOrder.BigEndian, true);
                            } else {
                                b = StreamFactory.CreateStream(w.Wav, w.Wav.fileHeader.vMajor, w.Wav.fileHeader.vMinor, w.Wav.fileHeader.vRevision).ToBytes(ByteOrder.LittleEndian);
                            }
                            break;

                        //SDK Stream Switch.
                        case 5:
                            b = StreamFactory.CreateStream(w.Wav, w.Wav.fileHeader.vMajor, w.Wav.fileHeader.vMinor, w.Wav.fileHeader.vRevision).ToBytes(ByteOrder.LittleEndian, true);
                            break;

                    }

                    //Write if possible.
                    if (b != null) {
                        System.IO.File.WriteAllBytes(s.FileName, b);
                    }

                }

            } else {
                MessageBox.Show("你不能导出空文件!", "通知:");
            }
        }

        /// <summary>
        /// Nullify the node.
        /// </summary>
        public override void NodeNullify() {
            (File as SoundWaveArchive)[tree.SelectedNode.Index] = null;
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>
        /// Delete the node.
        /// </summary>
        public override void NodeDelete() {
            (File as SoundWaveArchive).RemoveAt(tree.SelectedNode.Index);
            UpdateNodes();
            try {
                tree.SelectedNode = tree.Nodes["waves"].Nodes[tree.SelectedNode.Index - 1];
            } catch {
                tree.SelectedNode = tree.Nodes["waves"];
            }          
            DoInfoStuff();
        }

        #endregion


        //War tools.
        #region WarTools

        /// <summary>
        /// Extract waves.
        /// </summary>
        public override void WarExtractWave() {

            //File open check.
            if (!FileTest(null, null, false, true)) {
                return;
            }

            //Safety.
            string path = GetFolderPath();
            if (path == "") {
                return;
            }

            //Write each wave.
            int count = 0;
            foreach (var w in (File as SoundWaveArchive)) {

                if (w == null) {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + " (空的).wav", new byte[0]);
                } else {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + ".wav", RiffWaveFactory.CreateRiffWave(w.Wav).ToBytes());
                }

                count++;
            }

        }

        /// <summary>
        /// Extract 3ds.
        /// </summary>
        public override void WarExtractWave3ds() {

            //File open check.
            if (!FileTest(null, null, false, true)) {
                return;
            }

            //Safety.
            string path = GetFolderPath();
            if (path == "") {
                return;
            }

            //Write each wave.
            int count = 0;
            foreach (var w in (File as SoundWaveArchive)) {

                if (w == null) {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + " (空的).bcwav", new byte[0]);
                } else {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + ".bcwav", w.Wav.ToBytes(ByteOrder.LittleEndian));
                }

                count++;
            }

        }

        /// <summary>
        /// Extract WiiU.
        /// </summary>
        public override void WarExtractWaveWiiU() {

            //File open check.
            if (!FileTest(null, null, false, true)) {
                return;
            }

            //Safety.
            string path = GetFolderPath();
            if (path == "") {
                return;
            }

            //Write each wave.
            int count = 0;
            foreach (var w in (File as SoundWaveArchive)) {

                if (w == null) {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + " (空的).bfwav", new byte[0]);
                } else {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + ".bfwav", w.Wav.ToBytes(ByteOrder.BigEndian, true));
                }

                count++;
            }

        }

        /// <summary>
        /// Extract Switch.
        /// </summary>
        public override void WarExtractWaveSwitch() {

            //File open check.
            if (!FileTest(null, null, false, true)) {
                return;
            }

            //Safety.
            string path = GetFolderPath();
            if (path == "") {
                return;
            }

            //Write each wave.
            int count = 0;
            foreach (var w in (File as SoundWaveArchive)) {

                if (w == null) {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + " (空的).bfwav", new byte[0]);
                } else {
                    System.IO.File.WriteAllBytes(path + "/" + count.ToString("D5") + " - " + w.Name + ".bfwav", w.Wav.ToBytes(ByteOrder.LittleEndian, true));
                }

                count++;
            }

        }

        /// <summary>
        /// Import files.
        /// </summary>
        public override void WarBatchImport() {

            //File open check.
            if (!FileTest(null, null, false, true)) {
                return;
            }

            //Safety.
            string path = GetFolderPath();
            if (path == "") {
                return;
            }

            //Sort files.
            List<string> files = Directory.EnumerateFiles(path).ToList();
            for (int i = files.Count - 1; i >= 0; i--) {
                if (!files[i].Contains("wav")) {
                    files.RemoveAt(i);
                }
            }

            //Read each file.
            foreach (string d in files) {

                if (Path.GetExtension(d).ToLower().Contains(".wav")) {

                    byte[] b = System.IO.File.ReadAllBytes(d);
                    if (b.Length > 0) {
                        RiffWave r = new RiffWave();
                        r.Load(b);
                        (File as SoundWaveArchive).Add(new Wave() { Wav = WaveFactory.CreateWave(r, true, forceWavMaj, forceWavMin, forceWavRev) });
                    } else {
                        (File as SoundWaveArchive).Add(null);
                    }

                } else {

                    byte[] b = System.IO.File.ReadAllBytes(d);
                    if (b.Length > 0) {
                        Wave a = new Wave();
                        a.Wav = new b_wav();
                        a.Wav.Load(b);
                        (File as SoundWaveArchive).Add(a);
                    } else {
                        (File as SoundWaveArchive).Add(null);
                    }

                }

            }

            //Update.
            UpdateNodes();
            DoInfoStuff();

        }

        /// <summary>
        /// Get a folder path.
        /// </summary>
        /// <returns></returns>
        public string GetFolderPath() {

            FolderBrowserDialog f = new FolderBrowserDialog();
            f.ShowDialog();
            return f.SelectedPath;

        }

        #endregion


        //Other functions.
        #region OtherFunctions

        /// <summary>
        /// Get a path to a wave.
        /// </summary>
        /// <returns>Wave path.</returns>
        public string GetWavePath() {

            OpenFileDialog o = new OpenFileDialog();
            o.RestoreDirectory = true;
            o.FileName = "";
            o.Filter = "支持的文件|*.bfwav;*.bcwav;*.bfstm;*.bcstm;*.wav";
            o.ShowDialog();
            return o.FileName;

        }

        /// <summary>
        /// Get the wave data from a file path.
        /// </summary>
        /// <returns>Wave data.</returns>
        public Wave GetWave() {

            string wavePath = GetWavePath();
            if (wavePath == "") {
                return null;
            }

            //Extension.
            string ext = Path.GetExtension(wavePath).ToLower();

            //RIFF.
            if (ext.StartsWith(".w")) {
                RiffWave r = new RiffWave();
                r.Load(System.IO.File.ReadAllBytes(wavePath));
                return new Wave() { Wav = WaveFactory.CreateWave(r, true, forceWavMaj, forceWavMin, forceWavRev) };
            }

            //Wave.
            else if (ext.EndsWith("wav")) {
                b_wav b = new b_wav();
                b.Load(System.IO.File.ReadAllBytes(wavePath));
                return new Wave() { Wav = b };
            }

            //Stream.
            else if (ext.EndsWith("stm")) {
                b_stm s = new b_stm();
                s.Load(System.IO.File.ReadAllBytes(wavePath));
                return new Wave() { Wav = WaveFactory.CreateWave(s, forceWavMaj, forceWavMin, forceWavRev) };
            }

            //Null.
            return null;

        }

        /// <summary>
        /// On closing.
        /// </summary>
        public override void OnClosing() {
            kill = true;
            waveOut.Stop();
            try { waveOut.Dispose(); } catch { }
        }

        #endregion

    }

}
