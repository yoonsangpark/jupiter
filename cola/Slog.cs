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

public static class SLog
{
    private static string logDirectory;
    private static RichTextBox richTextBox;

    public static void Initialize(string logDirectory, RichTextBox richTextBox)
    {
        SLog.logDirectory = logDirectory;
        SLog.richTextBox = richTextBox;

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public static void log(Level lvl, string message)
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

        if (richTextBox != null)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => AppendTextWithColor(logLine + Environment.NewLine, color)));
            }
            else
            {
                AppendTextWithColor(logLine + Environment.NewLine, color);
            }
        }

        // 파일에 저장
        string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
        string filePath = Path.Combine(logDirectory, fileName);
        File.AppendAllText(filePath, logLine + Environment.NewLine);
    }

    private static void AppendTextWithColor(string text, Color color)
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