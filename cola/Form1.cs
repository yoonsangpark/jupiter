using System.Windows.Forms;

namespace cola
{
    public partial class Form1 : Form
    {
        private SLog slog;

        public Form1()
        {
            InitializeComponent();

            slog = new SLog("logs", RTB);
            slog.log(Level.INFO, "Application started.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void RTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void BTN_Click(object sender, EventArgs e)
        {
            slog.log(Level.INFO, "BTN_Click started.");
        }
    }
}
