using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianLab.Utilities
{
    internal class Config
    {
        public Dictionary<string, ConfigSection> sections = new Dictionary<string, ConfigSection>();
        string SecHead = "⩶";
        string SplitMark = "|";
        string EqualMark = "=";
        bool ConfirmDataMessage = false;
        public void ParseConfig(string filePath)
        {
            bool pastNull = false;
            sections = new Dictionary<string, ConfigSection>();
            if (File.Exists(filePath))
            {
                var read = new StreamReader(filePath);
                string line = SafeRead(read, out pastNull);
                while (!line.Contains(SecHead) && !pastNull)
                {
                    line = SafeRead(read, out pastNull);
                }
                while (!pastNull)
                {
                    string sectionName = line.Substring(line.IndexOf(SecHead) + SecHead.Length).Trim();
                    ConfigSection section = new ConfigSection();
                    Debug.WriteLine($"Parsing new section: {sectionName}...");
                    line = SafeRead(read, out pastNull);
                    if (pastNull) return;
                    while (!line.Contains(SecHead) && !pastNull)
                    {
                        if (line.Contains(EqualMark))
                        {
                            int eq_pnt = line.IndexOf(EqualMark) + EqualMark.Length;
                            Debug.WriteLine("Equal Position: " + eq_pnt.ToString());
                            string settingName = line.Substring(0, eq_pnt - 1);
                            settingName = settingName.Trim();
                            Setting setting = new Setting()
                            {
                                data = line.Substring(eq_pnt).Split(SplitMark, StringSplitOptions.TrimEntries),
                            };
                            Debug.WriteLine($"Parsing data for setting [{settingName}]: {setting.data[0]}...");
                            section.settings.Add(settingName, setting);
                        }
                        line = SafeRead(read, out pastNull);
                    }
                    sections.Add(sectionName, section);
                }
                read.Close();
                read.Dispose();
            }
            else
            {
                MessageBox.Show("Failed to locate Config.txt");
            }

            if (ConfirmDataMessage)
            {
                string message = "";
                foreach (string sectionname in sections.Keys)
                {
                    message += $"=== SECTION: {sectionname}..." + Environment.NewLine;
                    int settingIndex = 1;
                    foreach (string settingname in sections[sectionname].settings.Keys)
                    {
                        string data_message = "";
                        foreach (string datapoint in sections[sectionname].settings[settingname].data)
                        {
                            data_message += datapoint + ", ";
                        }
                        message += $"SETTING: {settingname} === [{data_message}]";
                        if (settingIndex % 2 == 0) message += Environment.NewLine;
                        else message += " - - - ";
                        settingIndex++;
                    }
                }
                MessageBox.Show(message);
            }
        }

        public string SafeRead(StreamReader read, out bool pastNull)
        {
            pastNull = false;
            try
            {
                string? outLine = read.ReadLine();
                if (outLine == null)
                {
                    pastNull = true;
                    outLine = "";
                }
                else
                {
                    outLine = outLine.Split("//", StringSplitOptions.TrimEntries)[0];
                }
                return outLine;
            }
            catch
            {
                pastNull = true;
                return "";
            }
        }
        public void WriteConfig(string path)
        {
            var write = new StreamWriter(path);
            foreach (string section in sections.Keys)
            {
                write.WriteLine($"{SecHead} {section}");
                foreach (string setting in sections[section].settings.Keys)
                {
                    string data = "";
                    for (int i = 0; i < sections[section].settings[setting].data.Length; i++)
                    {
                        data += sections[section].settings[setting].data[i];
                        if (i < sections[section].settings[setting].data.Length - 1)
                        {
                            data += $" {SplitMark} ";
                        }
                    }
                    write.WriteLine($"{setting} {EqualMark} {data}");
                }
                write.WriteLine("");
            }
            write.Close();
            write.Dispose();
        }
        public void OverwriteData(string section, string setting, string data)
        {
            if (sections.ContainsKey(section))
            {
                if (sections[section].settings.ContainsKey(setting))
                {
                    ConfigSection newSection = new ConfigSection();
                    foreach (string settingName in sections[section].settings.Keys)
                    {
                        if (settingName == setting)
                        {
                            Setting newSetting = new Setting();
                            newSetting.data = data.Split(SplitMark, StringSplitOptions.TrimEntries);
                            newSection.settings.Add(settingName, newSetting);
                        }
                        else
                        {
                            newSection.settings.Add(settingName, sections[section].settings[settingName]);
                        }
                    }
                    sections[section] = newSection;
                }
            }
        }
    }
    public class ConfigSection
    {
        public Dictionary<string, Setting> settings = new Dictionary<string, Setting>();
    }
    public struct Setting
    {
        public string[] data;
    }
    
}
