using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public enum Level
{
    INFO,
    WARN,
    ERROR,
    DEBUG
}

public class SLog
{
    private string logDirectory;
    private RichTextBox richTextBox;

    public SLog(string logDirectory, RichTextBox richTextBox)
    {
        this.logDirectory = logDirectory;
        this.richTextBox = richTextBox;

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public void log(Level lvl, string message)
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string levelStr = lvl.ToString().ToUpper();
        string logLine = $"[{time}] [{levelStr}] {message}";

        // RichTextBox에 색상 표시
        Color color = Color.Black;
        switch (lvl)
        {
            case Level.INFO:
                color = Color.Red;
                break;
            case Level.WARN:
                color = Color.Orange;
                break;
            case Level.ERROR:
                color = Color.Blue;
                break;
            case Level.DEBUG:
            default:
                color = Color.Black;
                break;
        }

        if (richTextBox.InvokeRequired)
        {
            richTextBox.Invoke(new Action(() => AppendTextWithColor(logLine + Environment.NewLine, color)));
        }
        else
        {
            AppendTextWithColor(logLine + Environment.NewLine, color);
        }

        // 파일에 저장
        string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
        string filePath = Path.Combine(logDirectory, fileName);
        File.AppendAllText(filePath, logLine + Environment.NewLine);
    }

    private void AppendTextWithColor(string text, Color color)
    {
        int start = richTextBox.TextLength;
        richTextBox.AppendText(text);
        int end = richTextBox.TextLength;

        richTextBox.Select(start, end - start);
        richTextBox.SelectionColor = color;
        richTextBox.SelectionLength = 0;
        richTextBox.ScrollToCaret();
    }
}