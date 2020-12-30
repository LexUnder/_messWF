using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotChatWF
{

    public partial class MainForm : Form
    {
        
        // Глобальные переменные
        int lastMsgID = 0;
        AuthentificationForm AuthForm;
        RegistartionForm RegForm;
        public TextBox TextBox_username;
        public int int_token;


        public MainForm()
        {
            InitializeComponent();
        }

        private void updateLoop_Tick(object sender, EventArgs e)
        {      
            List<Message> allMSG = GetAllMessages();            
            if (allMSG.Count > listMessages.Items.Count)
            {
              int index = -1;
              listMessages.Items.Clear();
              foreach(Message m in allMSG)
              {
                ++index;
                listMessages.Items.Add($"[{m.username}] {m.text}\t{m.timestamp}\tID: {index + 1}");
              } 
            }
            else if (allMSG.Count < listMessages.Items.Count)
            {
              listMessages.Items.Clear();
              int index = -1;
              foreach(Message m in allMSG)
              {
                ++index;
                listMessages.Items.Add($"[{m.username}] {m.text}\t{m.timestamp}\tID: {index + 1}");
              }
            }            
        }

        private void btnSend_Click(object sender, EventArgs e) {
            if (int_token == 0)
      {
        MessageBox.Show("Please log in or register");
      }
      else 
      {
        if (fieldMessage.Text.IndexOf("/delete ") != -1 && fieldMessage.Text[0] == '/') // Если сообщения вида /delete ID, то удаляем сообщение с индексом ID
        {
          string[] request = fieldMessage.Text.Split(' ');
          if (request.Length == 2)
          {
            var deleteRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/api/Chat/" + $"{Convert.ToInt32(request[1])}"); // Реквест на api/Chat + ID
            deleteRequest.Method = "DELETE";
            deleteRequest.GetResponse();
            fieldMessage.Text = String.Empty;
          }
          else // Если в окошке есть что-то помимо команды и ID
          {
            MessageBox.Show("Bad request");
            fieldMessage.Text = String.Empty;
          }
        
        }
        else // Если это обычное сообщение
        {
          SendMessage(new Message()
          {
            username = fieldUsername.Text,
            text = fieldMessage.Text,
          });
          fieldMessage.Text = String.Empty;
        }
      }
    }

        // Отправляет сообщение на сервер
        void SendMessage(Message msg)
        {
            WebRequest req = WebRequest.Create("http://localhost:5000/api/chat");
            req.Method = "POST";
            string postData = JsonConvert.SerializeObject(msg);
            //byte[] bytes = Encoding.UTF8.GetBytes(postData);
            req.ContentType = "application/json";
            //req.ContentLength = bytes.Length;
            StreamWriter reqStream = new StreamWriter(req.GetRequestStream());
            reqStream.Write(postData);
            reqStream.Close();
            req.GetResponse();
        }

        List<Message> GetAllMessages() // Спрашиваем список всех сообщений с сервера
        {
          var getRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5000/api/chat");
          using (StreamReader readList = new StreamReader(getRequest.GetResponse().GetResponseStream()))
          {
            return JsonConvert.DeserializeObject<List<Message>>(readList.ReadToEnd());
          }
        }

        // Получает сообщение с сервера
        Message GetMessage(int id)
        {
            try
            {
                WebRequest req = WebRequest.Create($"http://localhost:5000/api/chat/{id}");
                WebResponse resp = req.GetResponse();
                string smsg = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                var readNF = JObject.Parse(smsg); // Представляем json из ответа в виде JObject        
                if ((string)readNF["title"] == "Not Found") return null; // Смотрим на значение поля title
                return JsonConvert.DeserializeObject<Message>(smsg);
            } catch {        
                return null;
            }
        }

    private void btnAuth_Click(object sender, EventArgs e)
    {
      AuthForm = new AuthentificationForm();
      AuthForm.mForm = this;      
      AuthForm.Show();
      this.Visible = false;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      int_token = 0;
      AuthForm = new AuthentificationForm();
      RegForm = new RegistartionForm();
      TextBox_username = fieldUsername;

    }

    private void btnReg_Click(object sender, EventArgs e)
    {
      RegForm = new RegistartionForm();
      RegForm.mForm = this;     
      RegForm.Show();
      this.Visible = false;
    }

    private void listMessages_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void MainForm_VisibleChanged(object sender, EventArgs e)
    {
      if (this.Visible == true && int_token != 0)
      {
        this.updateLoop.Enabled = true;
      }
    }

    private void fieldMessage_TextChanged(object sender, EventArgs e)
    {

    }
  }
  [Serializable]
    public class Message
    {
        public string username = "";
        public string text = "";
        public DateTime timestamp;
    }
}
