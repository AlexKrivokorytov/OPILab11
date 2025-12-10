using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

//
// ============================================================
//  ЗАДАНИЕ 2 — MULTITON (Singleton с фиксированным числом объектов)
//  Реализация LimitedSingleton — до 3 экземпляров
// ============================================================
//
public class LimitedSingleton
{
    private static readonly int MaxInstances = 3;
    private static readonly List<LimitedSingleton> instances = new List<LimitedSingleton>();
    private static int counter = 0;
    private static readonly object lockObj = new object();

    public int Id { get; }

    private LimitedSingleton(int id)
    {
        Id = id;
    }

    public static LimitedSingleton GetInstance()
    {
        lock (lockObj)
        {
            if (instances.Count < MaxInstances)
            {
                var inst = new LimitedSingleton(instances.Count + 1);
                instances.Add(inst);
                return inst;
            }

            var obj = instances[counter];
            counter = (counter + 1) % MaxInstances;
            return obj;
        }
    }
}

//
// ============================================================
//  ЗАДАНИЕ 1 + 3 — SINGLETON + СИСТЕМА ЛОГИРОВАНИЯ
// ============================================================
//

namespace SimpleTextViewer
{
    public class EventLogger
    {
        private static EventLogger instance_;
        private static readonly object lockObj = new object();
        private List<LogEntry> logs;

        private EventLogger()
        {
            logs = new List<LogEntry>();
        }

        public static EventLogger Instance
        {
            get
            {
                if (instance_ == null)
                {
                    lock (lockObj)
                    {
                        if (instance_ == null)
                            instance_ = new EventLogger();
                    }
                }
                return instance_;
            }
        }

        public void LogEvent(string eventType, int line, int position, string text)
        {
            logs.Add(new LogEntry
            {
                Time = DateTime.Now,
                EventType = eventType,
                Line = line,
                Position = position,
                Text = text
            });
        }

        public string GetAllLogs()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== ЖУРНАЛ ПОДІЙ ===\n");

            foreach (var log in logs)
            {
                sb.AppendLine($"[{log.Time:HH:mm:ss}] {log.EventType} | Рядок: {log.Line}, Позиція: {log.Position} | Текст: '{log.Text}'");
            }

            return sb.ToString();
        }

        public void ClearLogs()
        {
            logs.Clear();
        }

        public int GetLogCount()
        {
            return logs.Count;
        }
    }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public string EventType { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string Text { get; set; }
    }


    public class Form1 : Form
    {
        private TextBox fileNameTextBox;
        private TextBox sourceTextBox;
        private OpenFileDialog openFileDialog;
        private HttpClient httpClient;
        private int lastTextLength = 0;

        public Form1()
        {
            httpClient = new HttpClient();
            openFileDialog = new OpenFileDialog { Filter = "Text Files|*.txt|All files|*.*" };

            fileNameTextBox = new TextBox { Location = new System.Drawing.Point(100, 30), Size = new System.Drawing.Size(400, 23), ReadOnly = true };
            sourceTextBox = new TextBox { Location = new System.Drawing.Point(12, 60), Size = new System.Drawing.Size(760, 440), Multiline = true, ScrollBars = ScrollBars.Both };

            sourceTextBox.KeyPress += SourceTextBox_KeyPress;
            sourceTextBox.TextChanged += SourceTextBox_TextChanged;

            Label fileNameLabel = new Label { Text = "Ім'я файлу:", Location = new System.Drawing.Point(12, 33), AutoSize = true };

            MenuStrip menuStrip = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            ToolStripMenuItem saveUniqueLinesMenuItem = new ToolStripMenuItem("Save Unique Lines");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");

            fileMenu.DropDownItems.Add(openMenuItem);
            fileMenu.DropDownItems.Add(saveUniqueLinesMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);

            ToolStripMenuItem newsMenu = new ToolStripMenuItem("News");
            ToolStripMenuItem downloadNewsMenuItem = new ToolStripMenuItem("Download News");
            newsMenu.DropDownItems.Add(downloadNewsMenuItem);

            ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");
            ToolStripMenuItem cleanSpacesMenuItem = new ToolStripMenuItem("Clean Spaces");
            ToolStripMenuItem findEmailsMenuItem = new ToolStripMenuItem("Find Emails");
            ToolStripMenuItem findComplexConstantsMenuItem = new ToolStripMenuItem("Find Complex Constants");
            ToolStripMenuItem findMostFrequentLatinWordsMenuItem = new ToolStripMenuItem("Most Frequent Latin Words");
            ToolStripMenuItem checkRealConstantsMenuItem = new ToolStripMenuItem("Check Real Constants");
            ToolStripMenuItem transformTextCaseMenuItem = new ToolStripMenuItem("Transform Text Case");

            editMenu.DropDownItems.Add(cleanSpacesMenuItem);
            editMenu.DropDownItems.Add(findEmailsMenuItem);
            editMenu.DropDownItems.Add(findComplexConstantsMenuItem);
            editMenu.DropDownItems.Add(findMostFrequentLatinWordsMenuItem);
            editMenu.DropDownItems.Add(checkRealConstantsMenuItem);
            editMenu.DropDownItems.Add(transformTextCaseMenuItem);

            ToolStripMenuItem aboutMenu = new ToolStripMenuItem("About");
            ToolStripMenuItem statisticsMenuItem = new ToolStripMenuItem("Statistics");
            ToolStripMenuItem viewLogsMenuItem = new ToolStripMenuItem("View Logs");
            ToolStripMenuItem clearLogsMenuItem = new ToolStripMenuItem("Clear Logs");

            // Новый пункт меню для теста MULTITON (задание 2)
            ToolStripMenuItem testMultitonMenuItem = new ToolStripMenuItem("Test Limited Singleton");

            aboutMenu.DropDownItems.Add(statisticsMenuItem);
            aboutMenu.DropDownItems.Add(new ToolStripSeparator());
            aboutMenu.DropDownItems.Add(viewLogsMenuItem);
            aboutMenu.DropDownItems.Add(clearLogsMenuItem);
            aboutMenu.DropDownItems.Add(new ToolStripSeparator());
            aboutMenu.DropDownItems.Add(testMultitonMenuItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(newsMenu);
            menuStrip.Items.Add(aboutMenu);

            openMenuItem.Click += OpenFile_Click;
            saveUniqueLinesMenuItem.Click += SaveUniqueLines_Click;
            exitMenuItem.Click += ExitApplication_Click;

            cleanSpacesMenuItem.Click += CleanSpaces_Click;
            findEmailsMenuItem.Click += FindEmails_Click;
            statisticsMenuItem.Click += ShowStats_Click;
            findComplexConstantsMenuItem.Click += FindComplexConstants_Click;
            findMostFrequentLatinWordsMenuItem.Click += FindMostFrequentLatinWords_Click;
            checkRealConstantsMenuItem.Click += CheckRealConstants_Click;
            transformTextCaseMenuItem.Click += TransformTextCase_Click;
            viewLogsMenuItem.Click += ViewLogs_Click;
            clearLogsMenuItem.Click += ClearLogs_Click;

            testMultitonMenuItem.Click += TestLimitedSingleton_Click;

            downloadNewsMenuItem.Click += async (s, ev) => await DownloadNews_Click(s, ev);

            this.Text = "Простий переглядач тексту";
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(fileNameLabel);
            this.Controls.Add(fileNameTextBox);
            this.Controls.Add(sourceTextBox);
        }


        // ============================================================
        //  ТЕСТ ЗАДАНИЯ 2 — Multiton
        // ============================================================
        private void TestLimitedSingleton_Click(object sender, EventArgs e)
        {
            var a = LimitedSingleton.GetInstance();
            var b = LimitedSingleton.GetInstance();
            var c = LimitedSingleton.GetInstance();
            var d = LimitedSingleton.GetInstance();

            MessageBox.Show($"A={a.Id}, B={b.Id}, C={c.Id}, D={d.Id}",
                "MULTITON TEST");
        }


        private void SourceTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int line = tb.GetLineFromCharIndex(tb.SelectionStart) + 1;
            int position = tb.SelectionStart;

            if (e.KeyChar == (char)Keys.Back)
            {
                if (position > 0 && position <= tb.Text.Length)
                {
                    string deletedChar = tb.Text[position - 1].ToString();
                    EventLogger.Instance.LogEvent("Видалення", line, position, deletedChar);
                }
            }
            else if (!char.IsControl(e.KeyChar))
            {
                EventLogger.Instance.LogEvent("Додавання", line, position, e.KeyChar.ToString());
            }
        }

        private void SourceTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            lastTextLength = tb.Text.Length;
        }


        private void ViewLogs_Click(object sender, EventArgs e)
        {
            string logs = EventLogger.Instance.GetAllLogs();

            Form logForm = new Form();
            logForm.Text = "Журнал подій";
            logForm.Width = 800;
            logForm.Height = 600;
            logForm.StartPosition = FormStartPosition.CenterParent;

            TextBox logTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Text = logs,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            logForm.Controls.Add(logTextBox);
            logForm.ShowDialog();

            MessageBox.Show($"Всього подій: {EventLogger.Instance.GetLogCount()}", "Інформація");
        }

        private void ClearLogs_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Ви впевнені, що хочете очистити всі логи?",
                "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                EventLogger.Instance.ClearLogs();
                MessageBox.Show("Логи успішно очищено!", "Інформація");
            }
        }


        // -------------REST---------

        private class NewsItem
        {
            public string Title { get; set; }
            public string Annotation { get; set; }
        }

        private async Task DownloadNews_Click(object sender, EventArgs e)
        {
            string input = ShowInputBox("Введіть кількість новин (1-50):", "Кількість", "10");
            if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int count) || count <= 0 || count > 50)
            {
                if (!string.IsNullOrEmpty(input)) MessageBox.Show("Введіть число від 1 до 50.");
                return;
            }

            try
            {
                var allNews = new List<NewsItem>();
                int page = 0;
                int newsPerPage = 10;

                while (allNews.Count < count)
                {
                    string url = $"https://www.znu.edu.ua/cms/index.php?action=news/view&start={page * newsPerPage}&site_id=27&lang=ukr";
                    var html = await httpClient.GetStringAsync(url);
                    var newsFromPage = ExtractNewsFromHtml(html, count - allNews.Count);

                    if (newsFromPage.Count == 0) break;

                    allNews.AddRange(newsFromPage);
                    page++;

                    if (page >= 10) break;
                }

                if (allNews.Count == 0)
                {
                    MessageBox.Show("Новин не знайдено!");
                    return;
                }

                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < allNews.Count; i++)
                    sb.AppendLine($"=== НОВИНА #{i + 1} ===\nЗаголовок: {allNews[i].Title}\n\nАнотація:\n{allNews[i].Annotation}\n\n");

                sourceTextBox.Text = sb.ToString();
                fileNameTextBox.Text = $"Новини ({allNews.Count})";
                MessageBox.Show($"Завантажено {allNews.Count} новин!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private string ShowInputBox(string prompt, string title, string defaultValue)
        {
            using (Form form = new Form())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new System.Drawing.Size(420, 110);
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                Label label = new Label() { Left = 10, Top = 10, Text = prompt, AutoSize = true, MaximumSize = new System.Drawing.Size(400, 0) };
                TextBox textBox = new TextBox() { Left = 10, Top = 40, Width = 380, Text = defaultValue };
                Button ok = new Button() { Text = "OK", Left = 230, Width = 75, Top = 70, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Скасувати", Left = 315, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };

                form.Controls.AddRange(new Control[] { label, textBox, ok, cancel });
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }

        private List<NewsItem> ExtractNewsFromHtml(string htmlContent, int maxCount)
        {
            var newsItems = new List<NewsItem>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            var newsNodes = doc.DocumentNode.SelectNodes("//div[@class='znu-2016-new-img-list-info']");
            if (newsNodes == null || newsNodes.Count == 0)
            {
                newsNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'znu-2016-new')]");
            }

            if (newsNodes == null) return newsItems;

            int count = 0;
            foreach (var node in newsNodes)
            {
                if (count >= maxCount) break;

                try
                {
                    var titleNode = node.SelectSingleNode(".//h4/a") ?? node.SelectSingleNode(".//h4");
                    var textNode =
                        node.SelectSingleNode(".//div[@class='text']/p") ??
                        node.SelectSingleNode(".//div[@class='text']") ??
                        node.SelectSingleNode(".//p");

                    if (titleNode != null)
                    {
                        string title = System.Net.WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                        string annotation = textNode != null
                            ? System.Net.WebUtility.HtmlDecode(textNode.InnerText.Trim())
                            : "Анотація відсутня";

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            newsItems.Add(new NewsItem { Title = title, Annotation = annotation });
                            count++;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            return newsItems;
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    FileInfo fi = new FileInfo(filePath);
                    sourceTextBox.Text = File.ReadAllText(filePath, System.Text.Encoding.GetEncoding(1251));
                    fileNameTextBox.Text = Path.GetFileName(filePath);

                    double sizeKB = fi.Length / 1024.0;
                    MessageBox.Show("Розмір файлу: " + sizeKB.ToString("F2") + " KB");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка читання файлу: " + ex.Message);
                }
            }
        }

        private void ShowStats_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Файл порожній або не відкритий!");
                return;
            }

            char[] wordSeparators = { ' ', '\r', '\n', '\t' };
            string[] lineSeparators = { "\r\n", "\n" };

            int characters = text.Length;
            int words = text.Split(wordSeparators, StringSplitOptions.RemoveEmptyEntries).Length;
            int paragraphs = text.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries).Length;

            string vowelsChars = "aeiouAEIOUаеєиіїоуюяАЕЄИІЇОУЮЯ";
            string consonantsChars = "bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZбвгґджзйклмнпрстфхцчшщБВГҐДЖЗЙКЛМНПРСТФХЦЧШЩ";
            int vowels = text.Count(c => vowelsChars.Contains(c));
            int consonants = text.Count(c => consonantsChars.Contains(c));

            double authorPages = characters / 1800.0;

            string stats = $"Символів: {characters}\n" +
                           $"Слів: {words}\n" +
                           $"Абзаців: {paragraphs}\n" +
                           $"Голосних: {vowels}\n" +
                           $"Приголосних: {consonants}\n" +
                           $"Авторських сторінок: {authorPages:F2}";

            MessageBox.Show(stats, "Статистика");
        }

        private void CleanSpaces_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту для очищення!");
                return;
            }

            string cleanedText = Regex.Replace(text, @"[ ]{2,}", " ");
            var lines = cleanedText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l));

            sourceTextBox.Text = string.Join(Environment.NewLine, lines);
            MessageBox.Show("Зайві пробіли видалено!");
        }

        private void FindEmails_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту для пошуку email-адрес!");
                return;
            }

            Regex emailRegex = new Regex(@"[\w\.-]+@([\w-]+\.)+[\w-]{2,4}");
            var matches = emailRegex.Matches(text);

            if (matches.Count > 0)
                MessageBox.Show(string.Join(Environment.NewLine, matches.Cast<Match>().Select(m => m.Value)), "Email-адреси");
            else
                MessageBox.Show("Email-адреси не знайдено.");
        }

        private void SaveUniqueLines_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту для обробки унікальних рядків!");
                return;
            }

            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<string> uniqueLines = new List<string>();

            string prev = null;
            foreach (var l in lines)
            {
                if (l != prev)
                {
                    uniqueLines.Add(l);
                    prev = l;
                }
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Text Files|*.txt|All files|*.*";
            save.FileName = "unique_lines.txt";

            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save.FileName,
                    string.Join(Environment.NewLine, uniqueLines),
                    System.Text.Encoding.GetEncoding(1251));

                MessageBox.Show("Файл збережено!");
            }
        }

        private void FindComplexConstants_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту!");
                return;
            }

            Regex regex = new Regex(@"\b([+-]?\d+(\.\d+)?([+-]\d+(\.\d+)?)?i|[+-]?\d+(\.\d+)?i)\b");
            var matches = regex.Matches(text);

            if (matches.Count > 0)
                MessageBox.Show(string.Join(Environment.NewLine, matches.Cast<Match>().Select(m => m.Value)), "Комплексні константи");
            else
                MessageBox.Show("Не знайдено.");
        }

        private void FindMostFrequentLatinWords_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту!");
                return;
            }

            Regex regex = new Regex(@"\b[a-zA-Z]+\b");
            var words = regex.Matches(text).Cast<Match>().Select(m => m.Value.ToLower());
            var groups = words.GroupBy(w => w).OrderByDescending(g => g.Count());

            if (groups.Any())
            {
                int max = groups.First().Count();
                var list = groups.Where(g => g.Count() == max).Select(g => g.Key);
                MessageBox.Show(string.Join(", ", list) + $"\nКількість: {max}", "Найчастіші слова");
            }
            else
                MessageBox.Show("Слів не знайдено.");
        }

        private void CheckRealConstants_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту!");
                return;
            }

            Regex regex = new Regex("\"([+-]?\\d+\\.\\d+)\"|'([+-]?\\d+\\.\\d+)'");
            var matches = regex.Matches(text);

            if (matches.Count > 0)
            {
                List<string> list = new List<string>();

                foreach (Match m in matches)
                {
                    if (m.Groups[1].Success) list.Add($"\"{m.Groups[1].Value}\"");
                    if (m.Groups[2].Success) list.Add($"'{m.Groups[2].Value}'");
                }

                MessageBox.Show(string.Join(Environment.NewLine, list), "Знайдені константи");
            }
            else
                MessageBox.Show("Не знайдено.");
        }

        private void TransformTextCase_Click(object sender, EventArgs e)
        {
            string text = sourceTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Немає тексту!");
                return;
            }

            string lower = text.ToLower();
            var sb = new System.Text.StringBuilder(lower);

            bool capitalize = true;

            for (int i = 0; i < sb.Length; i++)
            {
                if (capitalize && char.IsLetter(sb[i]))
                {
                    sb[i] = char.ToUpper(sb[i]);
                    capitalize = false;
                }
                else if (sb[i] == '.' || sb[i] == '!' || sb[i] == '?')
                {
                    capitalize = true;
                }
            }

            sourceTextBox.Text = sb.ToString();
        }

        private void ExitApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
