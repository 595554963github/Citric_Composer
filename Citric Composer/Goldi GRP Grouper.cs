using CitraFileLoader;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Citric_Composer
{
    public partial class Goldi_GRP_Grouper : EditorBase
    {

        //Versions.
        FileWriter.Version seqVersion = new FileWriter.Version(1, 0, 0);
        FileWriter.Version bnkVersion = new FileWriter.Version(1, 0, 0);
        FileWriter.Version warVersion = new FileWriter.Version(1, 0, 0);
        FileWriter.Version wsdVersion = new FileWriter.Version(1, 0, 0);
        FileWriter.Version stpVersion = new FileWriter.Version(1, 0, 0);

        //Set dependencies.
        bool depsSet;

        public Goldi_GRP_Grouper(MainWindow mainWindow) : base(typeof(Group), "组", "grp", "戈尔迪聚合器", mainWindow)
        {
            InitializeComponent();
            Text = "戈尔迪聚合器";
            Icon = Properties.Resources.Goldi;
        }

        public Goldi_GRP_Grouper(string fileToOpen, MainWindow mainWindow) : base(typeof(Group), "组", "grp", "戈尔迪聚合器", fileToOpen, mainWindow)
        {
            InitializeComponent();
            Text = "戈尔迪聚合器-" + Path.GetFileName(fileToOpen);
            Icon = Properties.Resources.Goldi;

        }

        public Goldi_GRP_Grouper(SoundFile<ISoundFile> fileToOpen, MainWindow mainWindow) : base(typeof(Group), "组", "grp", "戈尔迪聚合器", fileToOpen, mainWindow)
        {
            InitializeComponent();
            string name = ExtFile.FileName;
            if (name == null)
            {
                name = "{空的文件名}";
            }
            Text = EditorName + " - " + name + "." + ExtFile.FileExtension;
            Icon = Properties.Resources.Goldi;

        }

        /// <summary>
        /// Load dependencies if in independent mode since the files are not linked.
        /// </summary>
        public void LoadDependencies()
        {

            //Check.
            if (MainWindow != null)
            {
                if (!depsSet && ExtFile == null && MainWindow.File != null)
                {

                    for (int i = 0; i < (File as Group).SoundFiles.Count; i++)
                    {

                        if ((File as Group).SoundFiles[i].FileId < (MainWindow.File as SoundArchive).Files.Count())
                        {
                            if ((MainWindow.File as SoundArchive).Files[(File as Group).SoundFiles[i].FileId].File == null)
                            {
                                (MainWindow.File as SoundArchive).Files[(File as Group).SoundFiles[i].FileId].File = (File as Group).SoundFiles[i].File;
                            }
                            if ((MainWindow.File as SoundArchive).Files[(File as Group).SoundFiles[i].FileId].Reference == null)
                            {
                                (File as Group).SoundFiles[i].Reference = (MainWindow.File as SoundArchive).Files[(File as Group).SoundFiles[i].FileId];
                                (MainWindow.File as SoundArchive).Files[(File as Group).SoundFiles[i].FileId].ReferencedBy.Add((File as Group).SoundFiles[i]);
                            }
                        }

                    }
                    depsSet = true;

                }
            }

        }


        //Info and updates.
        #region InfoAndUpdates

        /// <summary>
        /// Do info stuff.
        /// </summary>
        public override void DoInfoStuff()
        {
            if (!FileOpen || File == null || !(File is Group fileGroup) || tree == null || tree.IsDisposed)
            {
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "文件未加载或无效";
                return;
            }

            if (tree.SelectedNode == null)
            {
                noInfoPanel.BringToFront();
                noInfoPanel.Show();
                status.Text = "没有选择节点";
                return;
            }

            if (fileGroup.Version == null)
            {
                fileGroup.Version = new FileWriter.Version(1, 0, 0);
            }

            if (grpMajBox != null)
            {
                grpMajBox.Value = fileGroup.Version.Major;
            }

            if (grpMinBox != null)
            {
                grpMinBox.Value = fileGroup.Version.Minor;
            }

            if (grpRevBox != null)
            {
                grpRevBox.Value = fileGroup.Version.Revision;
            }

            if (tree.SelectedNode.Parent != null)
            {
                if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
                {
                    var d = fileGroup.ExtraInfo[tree.SelectedNode.Index];
                    if (d == null)
                    {
                        nullDataPanel.BringToFront();
                        nullDataPanel.Show();
                        status.Text = "未选择有效信息!";
                    }
                    else
                    {
                        grpDependencyPanel.BringToFront();
                        grpDependencyPanel.Show();
                        grpDepLoadFlagsBox.Items.Clear();
                        status.Text = "依赖关系: " + tree.SelectedNode.Index;
                        WritingInfo = true;
                        grpDepEntryNumComboBox.Items.Clear();
                        grpDepEntryNumComboBox.Items.Add("未知条目");
                        grpDepEntryNumBox.Value = d.ItemIndex;

                        switch (d.ItemType)
                        {
                            case InfoExEntry.EItemType.Sound:
                                grpDepEntryTypeBox.SelectedIndex = 0;
                                grpDepLoadFlagsBox.Items.Add("所有");
                                grpDepLoadFlagsBox.Items.Add("声音和音色库");
                                grpDepLoadFlagsBox.Items.Add("声音和波形档案");
                                grpDepLoadFlagsBox.Items.Add("音色库和波形档案");
                                grpDepLoadFlagsBox.Items.Add("仅声音");
                                grpDepLoadFlagsBox.Items.Add("仅音色库");
                                grpDepLoadFlagsBox.Items.Add("仅波形档案");
                                switch (d.LoadFlags)
                                {
                                    case InfoExEntry.ELoadFlags.SeqAndBank:
                                        grpDepLoadFlagsBox.SelectedIndex = 1;
                                        break;
                                    case InfoExEntry.ELoadFlags.SeqAndWarc:
                                        grpDepLoadFlagsBox.SelectedIndex = 2;
                                        break;
                                    case InfoExEntry.ELoadFlags.BankAndWarc:
                                        grpDepLoadFlagsBox.SelectedIndex = 3;
                                        break;
                                    case InfoExEntry.ELoadFlags.Seq:
                                        grpDepLoadFlagsBox.SelectedIndex = 4;
                                        break;
                                    case InfoExEntry.ELoadFlags.Bank:
                                        grpDepLoadFlagsBox.SelectedIndex = 5;
                                        break;
                                    case InfoExEntry.ELoadFlags.Warc:
                                        grpDepLoadFlagsBox.SelectedIndex = 6;
                                        break;
                                    default:
                                        grpDepLoadFlagsBox.SelectedIndex = 0;
                                        break;
                                }
                                break;
                            case InfoExEntry.EItemType.SequenceSetOrWaveData:
                                grpDepEntryTypeBox.SelectedIndex = 1;
                                grpDepLoadFlagsBox.Items.Add("所有");
                                grpDepLoadFlagsBox.Items.Add("音序集和音色库");
                                grpDepLoadFlagsBox.Items.Add("音序集和波形档案");
                                grpDepLoadFlagsBox.Items.Add("音色库和波形档案");
                                grpDepLoadFlagsBox.Items.Add("仅音序集");
                                grpDepLoadFlagsBox.Items.Add("仅音色库");
                                grpDepLoadFlagsBox.Items.Add("仅波形档案");
                                grpDepLoadFlagsBox.Items.Add("仅波形声音数据集");
                                switch (d.LoadFlags)
                                {
                                    case InfoExEntry.ELoadFlags.SeqAndBank:
                                        grpDepLoadFlagsBox.SelectedIndex = 1;
                                        break;
                                    case InfoExEntry.ELoadFlags.SeqAndWarc:
                                        grpDepLoadFlagsBox.SelectedIndex = 2;
                                        break;
                                    case InfoExEntry.ELoadFlags.BankAndWarc:
                                        grpDepLoadFlagsBox.SelectedIndex = 3;
                                        break;
                                    case InfoExEntry.ELoadFlags.Seq:
                                        grpDepLoadFlagsBox.SelectedIndex = 4;
                                        break;
                                    case InfoExEntry.ELoadFlags.Bank:
                                        grpDepLoadFlagsBox.SelectedIndex = 5;
                                        break;
                                    case InfoExEntry.ELoadFlags.Warc:
                                        grpDepLoadFlagsBox.SelectedIndex = 6;
                                        break;
                                    case InfoExEntry.ELoadFlags.Wsd:
                                        grpDepLoadFlagsBox.SelectedIndex = 7;
                                        break;
                                    default:
                                        grpDepLoadFlagsBox.SelectedIndex = 0;
                                        break;
                                }
                                break;
                            case InfoExEntry.EItemType.Bank:
                                grpDepEntryTypeBox.SelectedIndex = 2;
                                grpDepLoadFlagsBox.Items.Add("所有");
                                grpDepLoadFlagsBox.Items.Add("仅音色库");
                                grpDepLoadFlagsBox.Items.Add("仅波形档案");
                                switch (d.LoadFlags)
                                {
                                    case InfoExEntry.ELoadFlags.Bank:
                                        grpDepLoadFlagsBox.SelectedIndex = 1;
                                        break;
                                    case InfoExEntry.ELoadFlags.Warc:
                                        grpDepLoadFlagsBox.SelectedIndex = 2;
                                        break;
                                    default:
                                        grpDepLoadFlagsBox.SelectedIndex = 0;
                                        break;
                                }
                                break;
                            case InfoExEntry.EItemType.WaveArchive:
                                grpDepEntryTypeBox.SelectedIndex = 3;
                                grpDepLoadFlagsBox.Items.Add("All");
                                grpDepLoadFlagsBox.SelectedIndex = 0;
                                break;
                        }
                        WritingInfo = false;
                    }
                }
                else
                {
                    Group g = fileGroup;
                    if (g.SoundFiles[tree.SelectedNode.Index] == null)
                    {
                        nullDataPanel.BringToFront();
                        nullDataPanel.Show();
                    }
                    else
                    {
                        grpFilePanel.BringToFront();
                        grpFilePanel.Show();
                        WritingInfo = true;
                        grpEmbedModeBox.SelectedIndex = g.SoundFiles[tree.SelectedNode.Index].Embed ? 1 : 0;
                        int fileId = g.SoundFiles[tree.SelectedNode.Index].FileId;
                        grpFileIdBox.Value = fileId;
                        grpFileIdComboBox.Items.Clear();
                        grpFileIdComboBox.Items.Add("未知文件条目");

                        if (MainWindow != null)
                        {
                            if (MainWindow.File != null || ExtFile != null)
                            {
                                int index = 0;
                                foreach (var f in (MainWindow.File as SoundArchive).Files)
                                {
                                    if (f == null)
                                    {
                                        grpFileIdComboBox.Items.Add("{ NULL }");
                                    }
                                    else
                                    {
                                        string name = f.FileName;
                                        if (name == null) { name = "{空文件名称}"; }
                                        grpFileIdComboBox.Items.Add("[" + index + "] " + name + "." + f.FileExtension);
                                    }
                                    index++;
                                }
                                try
                                {
                                    grpFileIdComboBox.SelectedIndex = fileId + 1;
                                }
                                catch { }
                            }
                            else
                            {
                                grpFileIdComboBox.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            grpFileIdComboBox.SelectedIndex = 0;
                        }
                        WritingInfo = false;
                    }
                    status.Text = "文件条目: " + tree.SelectedNode.Index;
                }
            }
            else if (tree.SelectedNode == tree.Nodes["fileInfo"])
            {
                grpFileInfoPanel.BringToFront();
                grpFileInfoPanel.Show();
                WritingInfo = true;
                grpMajBox.Value = fileGroup.Version.Major;
                grpMinBox.Value = fileGroup.Version.Minor;
                grpRevBox.Value = fileGroup.Version.Revision;
                grpSeqMajBox.Value = seqVersion.Major;
                grpSeqMinBox.Value = seqVersion.Minor;
                grpSeqRevBox.Value = seqVersion.Revision;
                grpBnkMajBox.Value = bnkVersion.Major;
                grpBnkMinBox.Value = bnkVersion.Minor;
                grpBnkRevBox.Value = bnkVersion.Revision;
                grpWarMajBox.Value = warVersion.Major;
                grpWarMinBox.Value = warVersion.Minor;
                grpWarRevBox.Value = warVersion.Revision;
                grpWsdMajBox.Value = wsdVersion.Major;
                grpWsdMinBox.Value = wsdVersion.Minor;
                grpWsdRevBox.Value = wsdVersion.Revision;
                grpStpMajBox.Value = stpVersion.Major;
                grpStpMinBox.Value = stpVersion.Minor;
                grpStpRevBox.Value = stpVersion.Revision;
                WritingInfo = false;
                status.Text = "文件信息.";
            }

            if (tree.SelectedNode == tree.Nodes["dependencies"])
            {
                int count = fileGroup.ExtraInfo?.Count ?? 0;
                status.Text = $"依赖计数: {count}";
            }
            else if (tree.SelectedNode == tree.Nodes["files"])
            {
                int count = fileGroup.SoundFiles?.Count ?? 0;
                status.Text = $"文件条目计数: {count}";
            }
        }

        /// <summary>
        /// Update nodes.
        /// </summary>
        public override void UpdateNodes()
        {
            if (tree == null || tree.IsDisposed) return;

            //Begin update.
            BeginUpdateNodes();

            try
            {
                //Check.
                if (FileOpen)
                {
                    LoadDependencies();
                }
                else
                {
                    depsSet = false;
                }

                //Add dependencies if doesn't exist.
                if (tree.Nodes.Count < 2)
                {
                    tree.Nodes.Add("dependencies", "依赖", 7, 7);
                }

                //Add files if doesn't exist.
                if (tree.Nodes.Count < 3)
                {
                    tree.Nodes.Add("files", "文件", 11, 11);
                }

                //File is open and not null.
                if (FileOpen && File != null && File is Group)
                {
                    Group g = (File as Group);

                    //Root context menu.
                    tree.Nodes["dependencies"].ContextMenuStrip = rootMenu;
                    tree.Nodes["files"].ContextMenuStrip = rootMenu;

                    //Clear existing nodes to prevent duplicates
                    tree.Nodes["dependencies"].Nodes.Clear();
                    tree.Nodes["files"].Nodes.Clear();

                    //Add each dependency.
                    if (g.ExtraInfo != null)
                    {
                        for (int i = 0; i < g.ExtraInfo.Count; i++)
                        {
                            try
                            {
                                //Null entry.
                                if (g.ExtraInfo[i] == null)
                                {
                                    //Add null entry.
                                    tree.Nodes["dependencies"].Nodes.Add("dependency" + i, "[" + i + "] { Null Dependency }", 0, 0);
                                }
                                else
                                {
                                    //Get icon.
                                    int icon = 0;
                                    switch (g.ExtraInfo[i].ItemType)
                                    {
                                        //Bank.
                                        case InfoExEntry.EItemType.Bank:
                                            icon = 5;
                                            break;

                                        //Sequence.
                                        case InfoExEntry.EItemType.Sound:
                                            icon = 3;
                                            break;

                                        //Sequence set or wave data.
                                        case InfoExEntry.EItemType.SequenceSetOrWaveData:
                                            icon = 4;
                                            break;

                                        //Wave archive.
                                        case InfoExEntry.EItemType.WaveArchive:
                                            icon = 6;
                                            break;
                                    }

                                    //Try to get dependency name.
                                    string depName = "{未知的依赖项名称}";
                                    if (MainWindow != null && (MainWindow.File != null || ExtFile != null))
                                    {
                                        try
                                        {
                                            //Item type.
                                            switch (g.ExtraInfo[i].ItemType)
                                            {
                                                //Sequence.
                                                case InfoExEntry.EItemType.Sound:
                                                    int index = g.ExtraInfo[i].ItemIndex;
                                                    var soundArchive = MainWindow.File as SoundArchive;

                                                    if (soundArchive != null)
                                                    {
                                                        //Stream.
                                                        if (index < soundArchive.Streams.Count)
                                                        {
                                                            icon = 1;
                                                            depName = soundArchive.Streams[index].Name;
                                                        }
                                                        //Wave sound data.
                                                        else if (index >= soundArchive.Streams.Count &&
                                                                 index < soundArchive.Streams.Count + soundArchive.WaveSoundDatas.Count)
                                                        {
                                                            icon = 2;
                                                            depName = soundArchive.WaveSoundDatas[index - soundArchive.Streams.Count].Name;
                                                        }
                                                        //Sequence.
                                                        else if (index >= soundArchive.Streams.Count + soundArchive.WaveSoundDatas.Count)
                                                        {
                                                            icon = 3;
                                                            depName = soundArchive.Sequences[index - soundArchive.WaveSoundDatas.Count - soundArchive.Streams.Count].Name;
                                                        }
                                                    }
                                                    break;

                                                //Bank.
                                                case InfoExEntry.EItemType.Bank:
                                                    depName = (MainWindow.File as SoundArchive)?.Banks[g.ExtraInfo[i].ItemIndex]?.Name;
                                                    break;

                                                //Wave archive.
                                                case InfoExEntry.EItemType.WaveArchive:
                                                    depName = (MainWindow.File as SoundArchive)?.WaveArchives[g.ExtraInfo[i].ItemIndex]?.Name;
                                                    break;

                                                //Sequence set or wave data.
                                                case InfoExEntry.EItemType.SequenceSetOrWaveData:
                                                    depName = (MainWindow.File as SoundArchive)?.SoundSets[g.ExtraInfo[i].ItemIndex]?.Name;
                                                    break;
                                            }
                                        }
                                        catch { }

                                        if (depName == null) { depName = "{ Null Name }"; }
                                    }

                                    //Add entry.
                                    tree.Nodes["dependencies"].Nodes.Add("dependency" + i, "[" + i + "] " + depName, icon, icon);
                                }

                                //Add context menu.
                                if (tree.Nodes["dependencies"].Nodes.ContainsKey("dependency" + i))
                                {
                                    tree.Nodes["dependencies"].Nodes["dependency" + i].ContextMenuStrip = CreateMenuStrip(nodeMenu,
                                        new int[] { 0, 1, 2, 3, 4, 7, 8 },
                                        new EventHandler[] {
                                    new EventHandler(this.addAboveToolStripMenuItem1_Click),
                                    new EventHandler(this.addBelowToolStripMenuItem1_Click),
                                    new EventHandler(this.moveUpToolStripMenuItem1_Click),
                                    new EventHandler(this.moveDownToolStripMenuItem1_Click),
                                    new EventHandler(blankToolStripMenuItem_Click),
                                    new EventHandler(replaceFileToolStripMenuItem_Click),
                                    new EventHandler(exportToolStripMenuItem1_Click),
                                    new EventHandler(nullifyToolStripMenuItem1_Click),
                                    new EventHandler(deleteToolStripMenuItem1_Click)
                                        });
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"处理依赖项时出错{i}: {ex.Message}");
                            }
                        }
                    }

                    //Load files.
                    if (g.SoundFiles != null)
                    {
                        int fCount = 0;
                        foreach (var f in g.SoundFiles)
                        {
                            try
                            {
                                //Null.
                                if (f == null)
                                {
                                    tree.Nodes["files"].Nodes.Add("file" + fCount, "[" + fCount + "] " + "{ Null File }", 0, 0);
                                }
                                else
                                {
                                    //File is valid.
                                    if (f.File != null)
                                    {
                                        //Get version.
                                        switch (f.File.GetExtension().Substring(f.File.GetExtension().Length - 3, 3).ToLower())
                                        {
                                            case "seq":
                                                seqVersion.Major = (f.File as SoundSequence).Version.Major;
                                                seqVersion.Minor = (f.File as SoundSequence).Version.Minor;
                                                seqVersion.Revision = (f.File as SoundSequence).Version.Revision;
                                                break;

                                            case "bnk":
                                                bnkVersion.Major = (f.File as SoundBank).Version.Major;
                                                bnkVersion.Minor = (f.File as SoundBank).Version.Minor;
                                                bnkVersion.Revision = (f.File as SoundBank).Version.Revision;
                                                break;

                                            case "war":
                                                warVersion.Major = (f.File as SoundWaveArchive).Version.Major;
                                                warVersion.Minor = (f.File as SoundWaveArchive).Version.Minor;
                                                warVersion.Revision = (f.File as SoundWaveArchive).Version.Revision;
                                                break;

                                            case "wsd":
                                                wsdVersion.Major = (f.File as WaveSoundData).Version.Major;
                                                wsdVersion.Minor = (f.File as WaveSoundData).Version.Minor;
                                                wsdVersion.Revision = (f.File as WaveSoundData).Version.Revision;
                                                break;

                                            case "stp":
                                                stpVersion.Major = (f.File as PrefetchFile).Version.Major;
                                                stpVersion.Minor = (f.File as PrefetchFile).Version.Minor;
                                                stpVersion.Revision = (f.File as PrefetchFile).Version.Revision;
                                                break;
                                        }
                                    }

                                    string name = f.FileName;
                                    if (name == null)
                                    {
                                        name = "{ Null File Name }";

                                        //Try and get name.
                                        if (MainWindow != null && (MainWindow.File != null || ExtFile != null))
                                        {
                                            try
                                            {
                                                //Set name.
                                                if ((MainWindow.File as SoundArchive)?.Files[f.FileId]?.FileName != null)
                                                {
                                                    name = (MainWindow.File as SoundArchive).Files[f.FileId].FileName;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    name += "." + f.FileExtension;

                                    //Get the icon.
                                    int icon = 0;
                                    if (f.FileExtension.Length > 3)
                                    {
                                        switch (f.FileExtension.Substring(2, 3))
                                        {
                                            case "seq":
                                                icon = 3;
                                                break;

                                            case "stm":
                                                icon = 1;
                                                break;

                                            case "wsd":
                                                icon = 2;
                                                break;

                                            case "bnk":
                                                icon = 5;
                                                break;

                                            case "war":
                                                icon = 6;
                                                break;

                                            case "stp":
                                                icon = 9;
                                                break;

                                            case "grp":
                                                icon = 7;
                                                break;
                                        }
                                    }

                                    //Get content type.
                                    string type = "(嵌入式)";
                                    if (!f.Embed)
                                    {
                                        type = "(链接式)";
                                    }

                                    tree.Nodes["files"].Nodes.Add("file" + fCount, "[" + fCount + "] " + name + " " + type, icon, icon);
                                }

                                //Add context menu.
                                if (tree.Nodes["files"].Nodes.ContainsKey("file" + fCount))
                                {
                                    tree.Nodes["files"].Nodes["file" + fCount].ContextMenuStrip = CreateMenuStrip(nodeMenu,
                                        new int[] { 0, 1, 2, 3, 5, 6, 8 },
                                        new EventHandler[] {
                                    new EventHandler(this.addAboveToolStripMenuItem1_Click),
                                    new EventHandler(this.addBelowToolStripMenuItem1_Click),
                                    new EventHandler(this.moveUpToolStripMenuItem1_Click),
                                    new EventHandler(this.moveDownToolStripMenuItem1_Click),
                                    new EventHandler(blankToolStripMenuItem_Click),
                                    new EventHandler(replaceFileToolStripMenuItem_Click),
                                    new EventHandler(exportToolStripMenuItem1_Click),
                                    new EventHandler(nullifyToolStripMenuItem1_Click),
                                    new EventHandler(deleteToolStripMenuItem1_Click)
                                        });

                                    //Type is STP context menu.
                                    if (MainWindow != null && MainWindow.File != null && f != null && f.FileExtension.EndsWith("stp"))
                                    {
                                        //Add extra entry.
                                        tree.Nodes["files"].Nodes["file" + fCount].ContextMenuStrip.Items.Insert(4,
                                            new ToolStripMenuItem("从流中生成", treeIcons.Images[1],
                                            new EventHandler(this.GenerateFromStreamClick)));
                                    }
                                }

                                //Increment count.
                                fCount++;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"处理文件时出错{fCount}: {ex.Message}");
                                fCount++;
                            }
                        }
                    }
                }
                else
                {
                    //Remove context menus.
                    tree.Nodes["dependencies"].ContextMenuStrip = null;
                    tree.Nodes["files"].ContextMenuStrip = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"更新节点时出错: {ex.Message}");
            }
            finally
            {
                //End update.
                EndUpdateNodes();
            }
        }

        /// <summary>
        /// Node is double clicked.
        /// </summary>
        public override void NodeMouseDoubleClick()
        {

            //Safety check.
            if (!FileOpen || File == null)
            {
                return;
            }

            //Make sure node is valid.
            if (tree.SelectedNode.Parent != null)
            {

                //Editor to open.
                EditorBase b = null;

                //If the parent is a file.
                Group g = File as Group;
                if (tree.SelectedNode.Parent == tree.Nodes["files"])
                {

                    //Embedded.
                    if (g.SoundFiles[tree.SelectedNode.Index].Embed || (ExtFile != null && g.SoundFiles[tree.SelectedNode.Index].File != null))
                    {

                        //Get the extension.
                        switch (g.SoundFiles[tree.SelectedNode.Index].FileExtension.Substring(g.SoundFiles[tree.SelectedNode.Index].FileExtension.Length - 3, 3).ToLower())
                        {

                            //Wave archive.
                            case "war":
                                b = new Brewster_WAR_Brewer(g.SoundFiles[tree.SelectedNode.Index], null, this);
                                break;

                            //Sequence.
                            case "seq":
                                b = new Static_Sequencer(g.SoundFiles[tree.SelectedNode.Index], null, this);
                                break;

                        }

                    }

                    //Linked.
                    else
                    {
                        MessageBox.Show("链接文件没有可读取的文件数据，在独立模式下编辑组时亦是如此!", "通知:");
                    }

                }

                //Open the editor.
                if (b != null)
                {
                    b.Show();
                }

            }

        }

        #endregion


        //Forcing versions or updating.
        #region ForcingVersions

        /// <summary>
        /// Version changed.
        /// </summary>
        public override void GroupVersionChanged()
        {

            //Set version variables.
            (File as Group).Version.Major = (byte)grpMajBox.Value;
            (File as Group).Version.Minor = (byte)grpMinBox.Value;
            (File as Group).Version.Revision = (byte)grpRevBox.Value;

            seqVersion.Major = (byte)grpSeqMajBox.Value;
            seqVersion.Minor = (byte)grpSeqMinBox.Value;
            seqVersion.Revision = (byte)grpSeqRevBox.Value;

            bnkVersion.Major = (byte)grpBnkMajBox.Value;
            bnkVersion.Minor = (byte)grpBnkMinBox.Value;
            bnkVersion.Revision = (byte)grpBnkRevBox.Value;

            warVersion.Major = (byte)grpWarMajBox.Value;
            warVersion.Minor = (byte)grpWarMinBox.Value;
            warVersion.Revision = (byte)grpWarRevBox.Value;

            wsdVersion.Major = (byte)grpWsdMajBox.Value;
            wsdVersion.Minor = (byte)grpWsdMinBox.Value;
            wsdVersion.Revision = (byte)grpWsdRevBox.Value;

            stpVersion.Major = (byte)grpStpMajBox.Value;
            stpVersion.Minor = (byte)grpStpMinBox.Value;
            stpVersion.Revision = (byte)grpStpRevBox.Value;

        }

        /// <summary>
        /// Force version.
        /// </summary>
        public override void GroupForceSequenceVersion()
        {

            Group g = (File as Group);
            for (int i = 0; i < g.SoundFiles.Count; i++)
            {

                //Not null.
                if (g.SoundFiles[i] != null)
                {

                    if (g.SoundFiles[i].File != null)
                    {
                        if (g.SoundFiles[i].File as SoundSequence != null)
                        {
                            (g.SoundFiles[i].File as SoundSequence).Version = seqVersion;
                        }
                    }

                }

            }

        }

        /// <summary>
        /// Force version.
        /// </summary>
        public override void GroupForceBankVersion()
        {

            Group g = (File as Group);
            for (int i = 0; i < g.SoundFiles.Count; i++)
            {

                //Not null.
                if (g.SoundFiles[i] != null)
                {

                    if (g.SoundFiles[i].File != null)
                    {
                        if (g.SoundFiles[i].File as SoundBank != null)
                        {
                            (g.SoundFiles[i].File as SoundBank).Version = bnkVersion;
                        }
                    }

                }

            }

        }

        /// <summary>
        /// Force version.
        /// </summary>
        public override void GroupForceWaveArchiveVersion()
        {

            Group g = (File as Group);
            for (int i = 0; i < g.SoundFiles.Count; i++)
            {

                //Not null.
                if (g.SoundFiles[i] != null)
                {

                    if (g.SoundFiles[i].File != null)
                    {
                        if (g.SoundFiles[i].File as SoundWaveArchive != null)
                        {
                            (g.SoundFiles[i].File as SoundWaveArchive).Version = warVersion;
                        }
                    }

                }

            }

        }

        /// <summary>
        /// Force version.
        /// </summary>
        public override void GroupForceWaveSoundDataVersion()
        {

            Group g = (File as Group);
            for (int i = 0; i < g.SoundFiles.Count; i++)
            {

                //Not null.
                if (g.SoundFiles[i] != null)
                {

                    if (g.SoundFiles[i].File != null)
                    {
                        if (g.SoundFiles[i].File as WaveSoundData != null)
                        {
                            (g.SoundFiles[i].File as WaveSoundData).Version = wsdVersion;
                        }
                    }

                }

            }

        }

        /// <summary>
        /// Force version.
        /// </summary>
        public override void GroupForcePrefetchVersion()
        {

            Group g = (File as Group);
            for (int i = 0; i < g.SoundFiles.Count; i++)
            {

                //Not null.
                if (g.SoundFiles[i] != null)
                {

                    if (g.SoundFiles[i].File != null)
                    {
                        if (g.SoundFiles[i].File as PrefetchFile != null)
                        {
                            (g.SoundFiles[i].File as PrefetchFile).Version = stpVersion;
                        }
                    }

                }

            }

        }

        #endregion


        //File info.
        #region FileInfo

        public override void GroupFileIdEmbedModeChanged()
        {

            if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
            {
                (File as Group).SoundFiles[tree.SelectedNode.Index].Embed = grpEmbedModeBox.SelectedIndex > 0;

                //File cannot be null if embedded mode.
                if (grpEmbedModeBox.SelectedIndex > 0 && (File as Group).SoundFiles[tree.SelectedNode.Index].File == null)
                {

                    //Open any file
                    OpenFileDialog o = new OpenFileDialog();
                    o.RestoreDirectory = true;
                    o.Filter = "任何声音文件|*.*";
                    o.ShowDialog();

                    if (o.FileName != "")
                    {
                        (File as Group).SoundFiles[tree.SelectedNode.Index].File = SoundArchiveReader.ReadFile(System.IO.File.ReadAllBytes(o.FileName));
                    }
                    else
                    {
                        (File as Group).SoundFiles[tree.SelectedNode.Index].Embed = false;
                    }

                }

                DoInfoStuff();
                UpdateNodes();

            }

        }

        public override void GroupFileIdComboChanged()
        {

            if (grpFileIdComboBox.SelectedIndex != 0 && (File as Group).SoundFiles[tree.SelectedNode.Index] != null)
            {

                if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                {

                    //Linked mode.
                    if (MainWindow != null)
                    {
                        if ((MainWindow.File != null || ExtFile != null) && (File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                        {

                            //Dereference files.
                            (File as Group).SoundFiles[tree.SelectedNode.Index].Reference.ReferencedBy.Remove((File as Group).SoundFiles[tree.SelectedNode.Index]);

                            //Set new reference.
                            (File as Group).SoundFiles[tree.SelectedNode.Index].Reference = (MainWindow.File as SoundArchive).Files[grpFileIdComboBox.SelectedIndex - 1];
                            (File as Group).SoundFiles[tree.SelectedNode.Index].Reference.ReferencedBy.Add((File as Group).SoundFiles[tree.SelectedNode.Index]);
                            DoInfoStuff();
                            UpdateNodes();

                        }
                        else if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                        {

                            //Just set file id.
                            (File as Group).SoundFiles[tree.SelectedNode.Index].FileId = grpFileIdComboBox.SelectedIndex - 1;
                            DoInfoStuff();
                            UpdateNodes();

                        }
                    }
                    //Independent mode.
                    else if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                    {

                        //Just set file id.
                        (File as Group).SoundFiles[tree.SelectedNode.Index].FileId = grpFileIdComboBox.SelectedIndex - 1;
                        DoInfoStuff();
                        UpdateNodes();

                    }

                }

            }

        }

        public override void GroupFileIdNumBoxChanged()
        {

            if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
            {

                //Linked mode.
                if (MainWindow != null)
                {
                    if (((MainWindow.File as SoundArchive) != null || ExtFile != null) && (File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                    {

                        //Dereference files.
                        (File as Group).SoundFiles[tree.SelectedNode.Index].Reference.ReferencedBy.Remove((File as Group).SoundFiles[tree.SelectedNode.Index]);

                        //Set new reference.
                        (File as Group).SoundFiles[tree.SelectedNode.Index].Reference = (MainWindow.File as SoundArchive).Files[(int)grpFileIdBox.Value];
                        (File as Group).SoundFiles[tree.SelectedNode.Index].Reference.ReferencedBy.Add((File as Group).SoundFiles[tree.SelectedNode.Index]);
                        DoInfoStuff();
                        UpdateNodes();

                    }

                    //Independent mode.
                    else if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                    {

                        //Just set file id.
                        (File as Group).SoundFiles[tree.SelectedNode.Index].FileId = (int)grpFileIdBox.Value;
                        DoInfoStuff();
                        UpdateNodes();

                    }
                }

                //Independent mode.
                else if ((File as Group).SoundFiles[tree.SelectedNode.Index] != null)
                {

                    //Just set file id.
                    (File as Group).SoundFiles[tree.SelectedNode.Index].FileId = (int)grpFileIdBox.Value;
                    DoInfoStuff();
                    UpdateNodes();

                }

            }

        }

        #endregion


        //Dependency info.
        #region DependencyInfo

        /// <summary>
        /// Change dependency type.
        /// </summary>
        public override void GroupDependencyTypeChanged()
        {

            InfoExEntry e = (File as Group).ExtraInfo[tree.SelectedNode.Index];
            e.ItemType = (InfoExEntry.EItemType)grpDepEntryTypeBox.SelectedIndex;
            e.LoadFlags = InfoExEntry.ELoadFlags.All;
            DoInfoStuff();
            UpdateNodes();

        }

        /// <summary>
        /// Flags changed.
        /// </summary>
        public override void GroupDependencyFlagsChanged()
        {

            //Switch type.
            InfoExEntry e = (File as Group).ExtraInfo[tree.SelectedNode.Index];
            switch (e.ItemType)
            {

                case InfoExEntry.EItemType.Sound:
                    switch (grpDepLoadFlagsBox.SelectedIndex)
                    {

                        case 1:
                            e.LoadFlags = InfoExEntry.ELoadFlags.SeqAndBank;
                            break;

                        case 2:
                            e.LoadFlags = InfoExEntry.ELoadFlags.SeqAndWarc;
                            break;

                        case 3:
                            e.LoadFlags = InfoExEntry.ELoadFlags.BankAndWarc;
                            break;

                        case 4:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Seq;
                            break;

                        case 5:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Bank;
                            break;

                        case 6:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Warc;
                            break;

                        default:
                            e.LoadFlags = InfoExEntry.ELoadFlags.All;
                            break;

                    }
                    break;

                case InfoExEntry.EItemType.SequenceSetOrWaveData:
                    switch (grpDepLoadFlagsBox.SelectedIndex)
                    {

                        case 1:
                            e.LoadFlags = InfoExEntry.ELoadFlags.SeqAndBank;
                            break;

                        case 2:
                            e.LoadFlags = InfoExEntry.ELoadFlags.SeqAndWarc;
                            break;

                        case 3:
                            e.LoadFlags = InfoExEntry.ELoadFlags.BankAndWarc;
                            break;

                        case 4:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Seq;
                            break;

                        case 5:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Bank;
                            break;

                        case 6:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Warc;
                            break;

                        case 7:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Wsd;
                            break;

                        default:
                            e.LoadFlags = InfoExEntry.ELoadFlags.All;
                            break;

                    }
                    break;

                case InfoExEntry.EItemType.Bank:
                    switch (grpDepLoadFlagsBox.SelectedIndex)
                    {

                        case 1:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Bank;
                            break;

                        case 2:
                            e.LoadFlags = InfoExEntry.ELoadFlags.Warc;
                            break;

                        default:
                            e.LoadFlags = InfoExEntry.ELoadFlags.All;
                            break;

                    }
                    break;

                case InfoExEntry.EItemType.WaveArchive:
                    e.LoadFlags = InfoExEntry.ELoadFlags.All;
                    break;

            }

        }

        /// <summary>
        /// Entry combo box changed.
        /// </summary>
        public override void GroupDependencyEntryComboChanged()
        {

            if (grpDepEntryNumComboBox.SelectedIndex != 0)
            {

                //Simply change the index.
                (File as Group).ExtraInfo[tree.SelectedNode.Index].ItemIndex = (int)grpDepEntryNumComboBox.SelectedIndex - 1;
                DoInfoStuff();
                UpdateNodes();

            }

        }

        /// <summary>
        /// Entry box changed.
        /// </summary>
        public override void GroupDependencyEntryNumBoxChanged()
        {

            //Simply change the index.
            (File as Group).ExtraInfo[tree.SelectedNode.Index].ItemIndex = (int)grpDepEntryNumBox.Value;
            DoInfoStuff();
            UpdateNodes();

        }

        #endregion


        //Node menus.
        #region NodeMenus

        /// <summary>
        /// Add an entry.
        /// </summary>
        public override void RootAdd()
        {

            //Dependency.
            if (tree.SelectedNode == tree.Nodes["dependencies"])
            {

                //Add info.
                (File as Group).ExtraInfo.Add(new InfoExEntry() { LoadFlags = InfoExEntry.ELoadFlags.All });
                UpdateNodes();

            }

            //File.
            else if (tree.SelectedNode == tree.Nodes["files"])
            {

                //Sound file.
                SoundFile<ISoundFile> s = new SoundFile<ISoundFile>();
                s.FileId = 0;
                s.Embed = false;

                //Reference.
                if (MainWindow != null)
                {
                    if (MainWindow.File != null || ExtFile != null)
                    {
                        s.Reference = (MainWindow.File as SoundArchive).Files[0];
                    }
                }

                //Add file.
                (File as Group).SoundFiles.Add(s);
                UpdateNodes();

            }

        }

        /// <summary>
        /// Add above.
        /// </summary>
        public override void NodeAddAbove()
        {

            //Dependency.
            if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
            {

                //Add info.
                (File as Group).ExtraInfo.Insert(tree.SelectedNode.Index, new InfoExEntry() { LoadFlags = InfoExEntry.ELoadFlags.All });
                UpdateNodes();
                tree.SelectedNode = tree.Nodes["dependencies"].Nodes[tree.SelectedNode.Index + 1];
                DoInfoStuff();

            }

            //File.
            else
            {

                //Sound file.
                SoundFile<ISoundFile> s = new SoundFile<ISoundFile>();
                s.FileId = 0;
                s.Embed = false;

                //Reference.
                if (MainWindow != null)
                {
                    if (MainWindow.File != null || ExtFile != null)
                    {
                        s.Reference = (MainWindow.File as SoundArchive).Files[0];
                    }
                }

                //Add file.
                (File as Group).SoundFiles.Insert(tree.SelectedNode.Index, s);
                UpdateNodes();
                tree.SelectedNode = tree.Nodes["files"].Nodes[tree.SelectedNode.Index + 1];
                DoInfoStuff();

            }

        }

        /// <summary>
        /// Add below.
        /// </summary>
        public override void NodeAddBelow()
        {

            //Dependency.
            if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
            {

                //Add info.
                (File as Group).ExtraInfo.Insert(tree.SelectedNode.Index + 1, new InfoExEntry() { LoadFlags = InfoExEntry.ELoadFlags.All });
                UpdateNodes();
                DoInfoStuff();

            }

            //File.
            else
            {

                //Sound file.
                SoundFile<ISoundFile> s = new SoundFile<ISoundFile>();
                s.FileId = 0;
                s.Embed = false;

                //Reference.
                if (MainWindow != null)
                {
                    if (MainWindow.File != null || ExtFile != null)
                    {
                        s.Reference = (MainWindow.File as SoundArchive).Files[0];
                    }
                }

                //Add file.
                (File as Group).SoundFiles.Insert(tree.SelectedNode.Index + 1, s);
                UpdateNodes();
                DoInfoStuff();

            }

        }

        /// <summary>
        /// Move up.
        /// </summary>
        public override void NodeMoveUp()
        {

            //Dependency.
            if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
            {
                if (Swap((File as Group).ExtraInfo, tree.SelectedNode.Index - 1, tree.SelectedNode.Index))
                {
                    UpdateNodes();
                    tree.SelectedNode = tree.Nodes["dependencies"].Nodes[tree.SelectedNode.Index - 1];
                    DoInfoStuff();
                }
            }

            //File.
            else
            {
                if (Swap((File as Group).SoundFiles, tree.SelectedNode.Index - 1, tree.SelectedNode.Index))
                {
                    UpdateNodes();
                    tree.SelectedNode = tree.Nodes["files"].Nodes[tree.SelectedNode.Index - 1];
                    DoInfoStuff();
                }
            }

        }

        /// <summary>
        /// Move down.
        /// </summary>
        public override void NodeMoveDown()
        {

            //Dependency.
            if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
            {
                if (Swap((File as Group).ExtraInfo, tree.SelectedNode.Index + 1, tree.SelectedNode.Index))
                {
                    UpdateNodes();
                    tree.SelectedNode = tree.Nodes["dependencies"].Nodes[tree.SelectedNode.Index + 1];
                    DoInfoStuff();
                }
            }

            //File.
            else
            {
                if (Swap((File as Group).SoundFiles, tree.SelectedNode.Index + 1, tree.SelectedNode.Index))
                {
                    UpdateNodes();
                    tree.SelectedNode = tree.Nodes["files"].Nodes[tree.SelectedNode.Index + 1];
                    DoInfoStuff();
                }
            }

        }

        /// <summary>
        /// Delete.
        /// </summary>
        public override void NodeDelete()
        {

            //Dependency.
            if (tree.SelectedNode.Parent == tree.Nodes["dependencies"])
            {
                (File as Group).ExtraInfo.RemoveAt(tree.SelectedNode.Index);
                UpdateNodes();
                try
                {
                    tree.SelectedNode = tree.Nodes["dependencies"].Nodes[tree.SelectedNode.Index - 1];
                }
                catch
                {
                    tree.SelectedNode = tree.Nodes["dependencies"];
                }
                DoInfoStuff();
            }

            //File.
            else
            {
                (File as Group).SoundFiles.RemoveAt(tree.SelectedNode.Index);
                UpdateNodes();
                try
                {
                    tree.SelectedNode = tree.Nodes["files"].Nodes[tree.SelectedNode.Index - 1];
                }
                catch
                {
                    tree.SelectedNode = tree.Nodes["files"];
                }
                DoInfoStuff();
            }

        }

        /// <summary>
        /// Export.
        /// </summary>
        public override void NodeExport()
        {

            //File data is not null.
            var f = (File as Group).SoundFiles[tree.SelectedNode.Index].File;
            WriteMode wM = WriteMode;
            if (f != null)
            {

                string path = GetFileSaverPath("声音文件", f.GetExtension().Substring(f.GetExtension().Length - 3, 3), ref wM);
                if (path != "")
                {

                    //Save file.
                    MemoryStream o = new MemoryStream();
                    BinaryDataWriter bw = new BinaryDataWriter(o);

                    //Write the file.
                    f.Write(wM, bw);

                    //Save the file.
                    System.IO.File.WriteAllBytes(path, o.ToArray());

                }

            }

            //Null.
            else
            {
                MessageBox.Show("你无法导出链接文件!");
            }

        }

        /// <summary>
        /// Replace.
        /// </summary>
        public override void NodeReplace()
        {

            //Open any file
            OpenFileDialog o = new OpenFileDialog();
            o.RestoreDirectory = true;
            o.Filter = "任何声音文件|*.*";
            o.ShowDialog();

            if (o.FileName != "")
            {
                (File as Group).SoundFiles[tree.SelectedNode.Index].File = SoundArchiveReader.ReadFile(System.IO.File.ReadAllBytes(o.FileName));
                DoInfoStuff();
                UpdateNodes();
            }

        }

        /// <summary>
        /// Blank.
        /// </summary>
        public override void NodeBlank()
        {
            (File as Group).ExtraInfo[tree.SelectedNode.Index] = new InfoExEntry() { LoadFlags = InfoExEntry.ELoadFlags.All };
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>
        /// Nullify.
        /// </summary>
        public override void NodeNullify()
        {
            (File as Group).ExtraInfo[tree.SelectedNode.Index] = null;
            UpdateNodes();
            DoInfoStuff();
        }

        /// <summary>
        /// Generate stream.
        /// </summary>
        private void GenerateFromStreamClick(object sender, EventArgs e)
        {

            MessageBox.Show("进行中!");

        }

        #endregion

    }
}
