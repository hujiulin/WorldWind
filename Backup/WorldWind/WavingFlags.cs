using System;
using System.Collections.Generic;
using System.IO;

using System.Xml;
using System.Xml.XPath;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;
using WorldWind.NewWidgets;
using Utility;

namespace WorldWind
{
    /// <summary>
    /// Summary description for WavingFOTW.
    /// </summary>
    public class WavingFlags : WorldWind.PluginEngine.Plugin
    {
        string DataFileUri = "http://worldwind26.arc.nasa.gov/fotw/fotw.txt";
        string FlagTextureDirectoryUri = "http://worldwind26.arc.nasa.gov/fotw/256x256";
        string FlagSuffix = "-lgflag.dds";
        string SavedFlagsDirectory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Plugins\\FOTW\\Textures";
        string SavedFilePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Plugins\\FOTW\\fotw.txt";
        RenderableObjectList m_wavingFlagsList = null;

        public System.Collections.Hashtable CountryHash = null;
        public string[] Headers = null;
        public int m_currentCategoryIndex = 2;
    
        private int[] m_headerIngoreIndices = new int[] {0, 1, 3, 4};
        FormWidget m_ciaForm = null;

        /// <summary>
        /// Plugin entry point - All plugins must implement this function
        /// </summary>
        public override void Load()
        {
            FileInfo savedFile = new FileInfo(SavedFilePath);
            if (!savedFile.Exists)
            {
                if (!savedFile.Directory.Exists)
                    savedFile.Directory.Create();

                try
                {
                    WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(DataFileUri);
                    download.DownloadFile(savedFile.FullName);
                    download.Dispose();
                }
                catch { }
            }

            m_wavingFlagsList = new RenderableObjectList("Waving Flags");
            m_wavingFlagsList.IsOn = false;
            System.Collections.Hashtable countryHash = new System.Collections.Hashtable();

            using (StreamReader reader = savedFile.OpenText())
            {
                string header = reader.ReadLine();
                string[] headers = header.Split('\t');

                string line = reader.ReadLine();
                while (line != null)
                {
                    System.Collections.Hashtable fieldHash = new System.Collections.Hashtable();
                    string[] lineParts = line.Split('\t');

                    //Log.Write(string.Format("{0}\t{1}", lineParts[0], lineParts[1]));
                    try
                    {
                        double latitude = double.Parse(lineParts[3], System.Globalization.CultureInfo.InvariantCulture);
                        double longitude = double.Parse(lineParts[4], System.Globalization.CultureInfo.InvariantCulture);

                        if (lineParts[1].Length == 2)
                        {
                            string flagFileUri = FlagTextureDirectoryUri + "/" + lineParts[1] + FlagSuffix;
                            FileInfo savedFlagFile = new FileInfo(SavedFlagsDirectory + "\\" + lineParts[1] + ".dds");

                            WavingFlagLayer flag = new WavingFlagLayer(
                                lineParts[0],
                                ParentApplication.WorldWindow.CurrentWorld,
                                latitude,
                                longitude,
                                flagFileUri);

                            flag.SavedImagePath = savedFlagFile.FullName;
                            flag.ScaleX = 100000;
                            flag.ScaleY = 100000;
                            flag.ScaleZ = 100000;
                            flag.Bar3D = new Bar3D(flag.Name, flag.World, latitude, longitude, 0, flag.ScaleZ, System.Drawing.Color.Red);
                            flag.Bar3D.ScaleX = 0.3f * flag.ScaleX;
                            flag.Bar3D.ScaleY = 0.3f * flag.ScaleY;
                            flag.Bar3D.IsOn = false;
                            flag.RenderPriority = RenderPriority.Custom;

                            flag.OnMouseEnterEvent += new EventHandler(flag_OnMouseEnterEvent);
                            flag.OnMouseLeaveEvent += new EventHandler(flag_OnMouseLeaveEvent);
                            flag.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(flag_OnMouseUpEvent);
                            m_wavingFlagsList.Add(flag);

                            for (int i = 0; i < lineParts.Length; i++)
                            {
                                try
                                {
                                    double value = double.Parse(lineParts[i], System.Globalization.CultureInfo.InvariantCulture);
                                    fieldHash.Add(headers[i], value);
                                }
                                catch
                                {
                                    fieldHash.Add(headers[i], lineParts[i]);
                                }
                            }
                            countryHash.Add(lineParts[0], fieldHash);
                        }
                        else
                        {
                            //Log.Write(Log.Levels.Debug, "blank: " + lineParts[0]);
                        }
                    }
                    catch(Exception ex)
                    {
                        Log.Write(Log.Levels.Warning, string.Format("Exception: {0} - {1}", lineParts[0], ex.ToString()));
                    }

                    line = reader.ReadLine();
                }
                Headers = headers;
            }
            
            CountryHash = countryHash;
            
            InitializeCiaForm();

            ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(m_wavingFlagsList);
        }

        void flag_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (DrawArgs.NewRootWidget.OnMouseMove(e) || DrawArgs.RootWidget.OnMouseMove(e))
                return;

            m_ciaForm.Visible = true;
        }

        void flag_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            WavingFlagLayer wavingFlag = (WavingFlagLayer)sender;
            wavingFlag.ShowHighlight = false;
            
        }

        void flag_OnMouseEnterEvent(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs mea = new System.Windows.Forms.MouseEventArgs(
                System.Windows.Forms.MouseButtons.None,
                0, 
                DrawArgs.LastMousePosition.X, 
                DrawArgs.LastMousePosition.Y, 
                0);

            // hack check to make sure that a widget isn't in the way
            if (DrawArgs.NewRootWidget.OnMouseMove(mea) || DrawArgs.RootWidget.OnMouseMove(mea))
                return;

            WavingFlagLayer flag = (WavingFlagLayer)sender;
            if (m_wavingFlagsList.IsOn && flag.Initialized && flag.IsOn)
            {
                ChangeForm(flag.Name, m_currentCategoryIndex);
                //m_ciaForm.Visible = true;
            }

            for (int i = 0; i < m_wavingFlagsList.ChildObjects.Count; i++)
            {
                if (m_wavingFlagsList.ChildObjects[i] is WavingFlagLayer)
                {
                    WavingFlagLayer wavingFlag = (WavingFlagLayer)m_wavingFlagsList.ChildObjects[i];
                    if (wavingFlag.Name != flag.Name)
                        wavingFlag.ShowHighlight = false;
                    else
                        wavingFlag.ShowHighlight = true;
                }
            }
        }
        
        struct KeyDataPair : IComparable
        {
            public string Key;
            public double Data;

            public KeyDataPair(string key, double data)
            {
                Key = key;
                Data = data;
            }

            public int CompareTo(object o)
            {
                KeyDataPair other = (KeyDataPair)o;
                return Data.CompareTo(other.Data);
            }
        }

        private void SortField(string field)
        {
            List<KeyDataPair> sortedList = new List<KeyDataPair>();
            foreach (string country in CountryHash.Keys)
            {
                System.Collections.Hashtable fieldHash = (System.Collections.Hashtable)CountryHash[country];

                if (fieldHash.Contains(field))
                {
                    try
                    {
                        double d = (double)fieldHash[field];
                        KeyDataPair kp = new KeyDataPair(country, d);
                        sortedList.Add(kp);
                    }
                    catch
                    { }
                }
            }

            sortedList.Sort();
            SortedFieldData.Add(field, sortedList);
        }

        System.Collections.Hashtable SortedFieldData = new System.Collections.Hashtable();

        void ChangeForm(string country, int category)
        {
            m_countryNameLabel.Text = country;
            m_currentCategoryLabel.Text = Headers[category];

            System.Collections.Hashtable fieldHash = (System.Collections.Hashtable)CountryHash[country];
            
            object value = fieldHash[Headers[category]];

            if (value is double)
            {
                if (!SortedFieldData.Contains(Headers[category]))
                {
                    SortField(Headers[category]);
                }
                List<KeyDataPair> sortedDataList = (List<KeyDataPair>)SortedFieldData[Headers[category]];

                int index = -1;
                double minValue = -1;
                double maxValue = -1;

                for (int i = 0; i < sortedDataList.Count; i++)
                {
                    if (sortedDataList[i].Key == country)
                    {
                        index = i;
                    }
                    
                    if (minValue == -1 || sortedDataList[i].Data < minValue)
                        minValue = sortedDataList[i].Data;
                    if (maxValue == -1 || sortedDataList[i].Data > maxValue)
                        maxValue = sortedDataList[i].Data;
                }

                for(int i = 0; i < m_wavingFlagsList.ChildObjects.Count; i++)
                {
                    if (m_wavingFlagsList.ChildObjects[i] is WavingFlagLayer)
                    {
                        WavingFlagLayer wavingFlag = (WavingFlagLayer)m_wavingFlagsList.ChildObjects[i];
                        wavingFlag.Bar3D.UseScaling = true;
                        wavingFlag.Bar3D.ScalarMinimum = minValue;
                        wavingFlag.Bar3D.ScalarMaximum = maxValue;
                        bool foundScalar = false;
                        
                        for (int j = 0; j < sortedDataList.Count; j++)
                        {
                            if (sortedDataList[j].Key == wavingFlag.Name)
                            {
                                wavingFlag.Bar3D.ScalarValue = sortedDataList[j].Data;
                                foundScalar = true;
                                break;
                            }
                        }
                        if (!foundScalar)
                            wavingFlag.Bar3D.IsOn = false;
                        else
                            wavingFlag.Bar3D.IsOn = true;
                    }
                }

                m_currentBodyText.Text = "";
                int startIndex = index - (index % 2 == 0 ? 5 : 4);

                if (startIndex > sortedDataList.Count - 10)
                    startIndex = sortedDataList.Count - 10;

                int counter = 0;
                while (counter < 10)
                {
                    if (startIndex < 0)
                    {
                        startIndex++;
                        continue;
                    }

                    m_scrollbars[counter].Visible = true;
                    m_scrollbars[counter].Minimum = minValue;
                    m_scrollbars[counter].Maximum = maxValue;
                    m_scrollbars[counter].Value = sortedDataList[startIndex].Data;

                    m_listLabels[counter].Visible = true;
                    m_listLabels[counter].Text = string.Format("{0}.   {1}\n", startIndex + 1, sortedDataList[startIndex].Key);
                    counter++;
                    startIndex++;
                }
            }
            else
            {
                for (int i = 0; i < m_scrollbars.Length; i++)
                    m_scrollbars[i].Visible = false;

                for (int i = 0; i < m_listLabels.Length; i++)
                    m_listLabels[i].Visible = false;

                for (int i = 0; i < m_wavingFlagsList.ChildObjects.Count; i++)
                {
                    if (m_wavingFlagsList.ChildObjects[i] is WavingFlagLayer)
                    {
                        WavingFlagLayer wavingFlag = (WavingFlagLayer)m_wavingFlagsList.ChildObjects[i];
                        wavingFlag.Bar3D.IsOn = false;
                    }
                }
                m_currentBodyText.Text = (string)value;
            }
        }

        /// <summary>
        /// Unloads our plugin
        /// </summary>
        public override void Unload()
        {
            if (m_wavingFlagsList != null)
            {
                ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(m_wavingFlagsList.Name);
                m_wavingFlagsList.Dispose();
                m_wavingFlagsList = null;
            }
        }

        PictureBox m_bg_top = null;
        PictureBox m_bg_middle = null;
        PictureBox m_bg_bottom = null;

        PictureBox m_nav_bar_up = null;
        PictureBox m_nav_bar_down = null;
        PictureBox m_nav_flag_up = null;
        PictureBox m_nav_flag_down = null;

        PictureBox m_category_introduction = null;
        PictureBox m_category_geography = null;
        PictureBox m_category_people = null;
        PictureBox m_category_economy = null;
        PictureBox m_category_communications = null;
        PictureBox m_category_transportation = null;
        PictureBox m_category_military = null;

        PictureBox m_nav_left = null;
        PictureBox m_nav_right = null;
        PictureBox m_nav_close = null;

        TextLabel m_countryNameLabel = null;
        TextLabel m_currentCategoryLabel = null;
        TextLabel m_currentBodyText = null;
        Scrollbar[] m_scrollbars = null;
        TextLabel[] m_listLabels = null;

        private void InitializeCiaForm()
        {
            m_ciaForm = new FormWidget("CIA World Fact Book");
            m_ciaForm.Anchor = WorldWind.NewWidgets.WidgetEnums.AnchorStyles.Right;
            m_ciaForm.ClientSize = new System.Drawing.Size(512, 384);

            // make it right aligned
            m_ciaForm.Location = new System.Drawing.Point(
                DrawArgs.ParentControl.Width - m_ciaForm.ClientSize.Width,
                DrawArgs.ParentControl.Height / 2 - m_ciaForm.ClientSize.Height / 2);

            m_ciaForm.BackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
            m_ciaForm.HeaderHeight = 0;
            m_ciaForm.HorizontalResizeEnabled = false;
            m_ciaForm.VerticalResizeEnabled = false;
            m_ciaForm.BorderEnabled = false;
            m_ciaForm.HorizontalScrollbarEnabled = false;
            m_ciaForm.VerticalScrollbarEnabled = false;

            m_bg_top = CreatePictureBox("Data\\Icons\\Interface\\bg-top.png", 0, 0, m_ciaForm);
            m_bg_middle = CreatePictureBox("Data\\Icons\\Interface\\bg-middle-extra.png", 0, 64, m_ciaForm);
            m_bg_bottom = CreatePictureBox("Data\\Icons\\Interface\\bg-bottom.png", 0, 320, m_ciaForm);

            int navOffset = 15;
            int navY = 329;

            m_nav_bar_up = CreatePictureBox("Data\\Icons\\Interface\\nav-bar-up.png", 0 * 32 + navOffset, navY, m_ciaForm);
            m_nav_bar_down = CreatePictureBox("Data\\Icons\\Interface\\nav-bar-down.png", 1 * 32 + navOffset, navY, m_ciaForm);

            m_nav_flag_up = CreatePictureBox("Data\\Icons\\Interface\\nav-flag-up.png", 2 * 32 + navOffset, navY, m_ciaForm);
            m_nav_flag_down = CreatePictureBox("Data\\Icons\\Interface\\nav-flag-down.png", 3 * 32 + navOffset, navY, m_ciaForm);
            
            int categoryOffset = 4 * 32 + navOffset + 32;

            m_category_introduction = CreatePictureBox("Data\\Icons\\Interface\\catagory-introduction.png", 0 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_introduction.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_introduction_OnMouseUpEvent);

            m_category_geography = CreatePictureBox("Data\\Icons\\Interface\\catagory-geography.png", 1 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_geography.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_geography_OnMouseUpEvent);
            
            m_category_people = CreatePictureBox("Data\\Icons\\Interface\\catagory-people.png", 2 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_people.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_people_OnMouseUpEvent);
            
            m_category_economy = CreatePictureBox("Data\\Icons\\Interface\\catagory-economy.png", 3 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_economy.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_economy_OnMouseUpEvent);
            
            m_category_communications = CreatePictureBox("Data\\Icons\\Interface\\catagory-communications.png", 4 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_communications.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_communications_OnMouseUpEvent);
            
            m_category_transportation = CreatePictureBox("Data\\Icons\\Interface\\catagory-transportation.png", 5 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_transportation.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_transportation_OnMouseUpEvent);
            
            m_category_military = CreatePictureBox("Data\\Icons\\Interface\\catagory-military.png", 6 * 32 + categoryOffset, navY, m_ciaForm);
            m_category_military.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_category_military_OnMouseUpEvent);

            m_nav_left = CreatePictureBox("Data\\Icons\\Interface\\nav-left.png", 8 * 32 + categoryOffset, navY, m_ciaForm);
            m_nav_left.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_nav_left_OnMouseUpEvent);
            
            m_nav_right = CreatePictureBox("Data\\Icons\\Interface\\nav-right.png", 9 * 32 + categoryOffset, navY, m_ciaForm);
            m_nav_right.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_nav_right_OnMouseUpEvent);

            m_nav_close = CreatePictureBox("Data\\Icons\\Interface\\close.png", 16, 16, m_ciaForm);
            m_nav_close.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(m_nav_close_OnMouseUpEvent);

            m_countryNameLabel = new TextLabel();
            m_countryNameLabel.Name = "CountryName";
            m_countryNameLabel.Text = "Country Name";
            m_countryNameLabel.ForeColor = System.Drawing.Color.FromArgb(37, 64, 71); // dark forest green
            m_countryNameLabel.Location = new System.Drawing.Point(75, 65);
            m_countryNameLabel.ClientSize = new System.Drawing.Size(m_ciaForm.ClientSize.Width - 100, 100);
            m_countryNameLabel.Font = new System.Drawing.Font("Ariel", 16.0f, System.Drawing.FontStyle.Bold);
            m_countryNameLabel.Alignment = Alignment.Right;

            m_currentCategoryLabel = new TextLabel();
            m_currentCategoryLabel.Name = "CurrentCategory";
            m_currentCategoryLabel.Text = "";
            m_currentCategoryLabel.ForeColor = System.Drawing.Color.FromArgb(37, 64, 71); // dark forest green
            m_currentCategoryLabel.Location = new System.Drawing.Point(75, 90);
            m_currentCategoryLabel.ClientSize = new System.Drawing.Size(m_ciaForm.ClientSize.Width - 100, 100);
            m_currentCategoryLabel.Font = new System.Drawing.Font("Ariel", 14.0f, System.Drawing.FontStyle.Regular);
            m_currentCategoryLabel.Alignment = Alignment.Right;

            m_currentBodyText = new TextLabel();
            m_currentBodyText.Name = "CurrentBodyText";
            m_currentBodyText.Text = "";
            m_currentBodyText.ForeColor = System.Drawing.Color.FromArgb(37, 64, 71); // dark forest green
            m_currentBodyText.Location = new System.Drawing.Point(25, 110);
            m_currentBodyText.ClientSize = new System.Drawing.Size(m_ciaForm.ClientSize.Width - 40, 220);
            m_currentBodyText.Font = new System.Drawing.Font("Ariel", 8.0f, System.Drawing.FontStyle.Regular);
            m_currentBodyText.WordBreak = true;

            int offsetY = m_currentBodyText.Location.Y + 5;
            
            m_listLabels = new TextLabel[10];
            for (int i = 0; i < m_listLabels.Length; i++)
            {
                m_listLabels[i] = new TextLabel();
                m_listLabels[i].ClientSize = new System.Drawing.Size(250, 14);
                m_listLabels[i].Location = new System.Drawing.Point(m_currentBodyText.Location.X, offsetY + i * m_listLabels[i].ClientSize.Height);
                m_listLabels[i].ForeColor = System.Drawing.Color.FromArgb(37, 64, 71);
                m_listLabels[i].Font = new System.Drawing.Font("Ariel", 10.0f, System.Drawing.FontStyle.Regular);
                m_listLabels[i].Visible = false;
                m_ciaForm.ChildWidgets.Add(m_listLabels[i]);
            }

            m_scrollbars = new Scrollbar[10];
            for (int i = 0; i < m_scrollbars.Length; i++)
            {
                m_scrollbars[i] = new Scrollbar();
                m_scrollbars[i].ClientSize = new System.Drawing.Size(150, 7);
                m_scrollbars[i].Location = new System.Drawing.Point(m_currentBodyText.Location.X + 265, m_listLabels[i].Location.Y + 2);
                m_scrollbars[i].ForeColor = System.Drawing.Color.DarkOrange;
                m_scrollbars[i].Value = 0.0f;
                m_scrollbars[i].Visible = false;
                m_ciaForm.ChildWidgets.Add(m_scrollbars[i]);
            }

            m_ciaForm.ChildWidgets.Add(m_countryNameLabel);
            m_ciaForm.ChildWidgets.Add(m_currentCategoryLabel);
            m_ciaForm.ChildWidgets.Add(m_currentBodyText);

            m_ciaForm.Visible = false;
            DrawArgs.NewRootWidget.ChildWidgets.Add(m_ciaForm);
            DrawArgs.ParentControl.Resize += new EventHandler(ParentControl_Resize);
        }

        void m_category_military_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 77;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_transportation_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 71;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_communications_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 62;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_economy_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 31;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_people_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 9;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_geography_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 5;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_category_introduction_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex = 2;
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_nav_left_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex--;
            if (m_currentCategoryIndex < 0)
                m_currentCategoryIndex = Headers.Length - 1;
            
            while(IsHeaderIgnoreIndex(m_currentCategoryIndex))
            {
                m_currentCategoryIndex--;
                if (m_currentCategoryIndex < 0)
                    m_currentCategoryIndex = Headers.Length - 1;
            }
            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_nav_right_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_currentCategoryIndex++;
            if (m_currentCategoryIndex >= Headers.Length)
                m_currentCategoryIndex = 0;

            while(IsHeaderIgnoreIndex(m_currentCategoryIndex))
            {
                m_currentCategoryIndex++;
                if (m_currentCategoryIndex >= Headers.Length)
                    m_currentCategoryIndex = 0;
            }

            ChangeForm(m_countryNameLabel.Text, m_currentCategoryIndex);
        }

        void m_nav_close_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m_ciaForm != null)
            {
                m_ciaForm.Visible = false;

                for (int i = 0; i < m_wavingFlagsList.ChildObjects.Count; i++)
                {
                    if (m_wavingFlagsList.ChildObjects[i] is WavingFlagLayer)
                    {
                        WavingFlagLayer wavingFlag = (WavingFlagLayer)m_wavingFlagsList.ChildObjects[i];
                        wavingFlag.ShowHighlight = false;
                    }
                }
            }
        }

        bool IsHeaderIgnoreIndex(int index)
        {
            for(int i = 0; i < m_headerIngoreIndices.Length; i++)
            {
                if(index == m_headerIngoreIndices[i])
                    return true;
            }

            return false;
        }

        private PictureBox CreatePictureBox(string imageUri, int x, int y, FormWidget parentForm)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Location = new System.Drawing.Point(x, y);
            pictureBox.ImageUri = imageUri;

            pictureBox.ParentWidget = parentForm;
            parentForm.ChildWidgets.Add(pictureBox);
            return pictureBox;
        }

        void ParentControl_Resize(object sender, EventArgs e)
        {
            // keep it right aligned
            m_ciaForm.Location = new System.Drawing.Point(
                DrawArgs.ParentControl.Width - m_ciaForm.ClientSize.Width,
                DrawArgs.ParentControl.Height / 2 - m_ciaForm.ClientSize.Height / 2);
        }
    }

    
}
