using System.Windows.Forms;
using System.Threading.Tasks;

namespace cola
{
    public partial class Form1 : Form
    {
        private TcpServer tcpServer;
        private const int TCP_PORT = 8889;

        public Form1()
        {
            InitializeComponent();

            SLog.Initialize("logs", RTB);
            SLog.log(Level.INFO, "Application started.");

            // TCP 서버 초기화
            tcpServer = new TcpServer(TCP_PORT);

            // 서버들 시작
            StartServers();
        }

        private void StartServers()
        {
            try
            {
                // TCP 서버 시작
                tcpServer.Start();
                SLog.log(Level.INFO, $"TCP 서버 시작 중... (포트: {TCP_PORT})");
            }
            catch (Exception ex)
            {
                SLog.log(Level.ERROR, $"서버 시작 실패: {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void RTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void BTN_Click(object sender, EventArgs e)
        {
            SLog.log(Level.INFO, "BTN_Click started.");            
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // TCP 서버 정리
            if (tcpServer != null && tcpServer.IsRunning)
            {
                tcpServer.Stop();
                SLog.log(Level.INFO, "TCP 서버 정리 완료.");
            }
            base.OnFormClosing(e);
        }
    }
}
