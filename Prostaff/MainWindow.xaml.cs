using System;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Prostaff
{
    public partial class MainWindow : Window
    {
        public string path = String.Empty;
        public string Attachment1_path = null;
        public string Attachment2_path = null;

        private static Excel.Workbook MyBook = null;
        private static Excel.Application MyApp = null;
        private static Excel.Worksheet MySheet = null;

        List<string> list = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < EmailList.Items.Count; i++)
            {
                if (EmailList.Items[i] != null)
                {
                    MailAddress from = new MailAddress(adress.Text);

                    string email = Regex.Replace(EmailList.Items[i].ToString(), @"\s+", string.Empty);

                    MailAddress to = new MailAddress(email);
                    MailMessage message = new MailMessage(from, to);

                    message.Subject = subject.Text;
                    message.IsBodyHtml = true;
                    message.Body = TextMessage.Text;

                    if (Attachment1_path != null)
                        message.Attachments.Add(new Attachment(Attachment1_path));
                    if (Attachment2_path != null)
                        message.Attachments.Add(new Attachment(Attachment2_path));


                    SmtpClient client = new SmtpClient(server.Text);
                    client.Port = Int32.Parse(port.Text);
                    client.EnableSsl = true;

                    client.Credentials = new NetworkCredential(from.Address, password.Text);

                    try
                    {
                        client.Send(message);
                        Logs.Items.Add("Mail to " + email + " has been successfully sent");
                        EmailList.Items.Remove(EmailList.Items[i]);
                        EmailList.Items.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Logs.Items.Add("Couldn't send mail to " + email);
                        EmailList.Items.Remove(EmailList.Items[i]);
                        EmailList.Items.Refresh();
                    }
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            EmailList.Items.Remove(EmailList.SelectedItem);
            EmailList.Items.Refresh();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Logs.Items.Add("Importing e-mails...");

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "ExcelNames|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                path = dlg.FileName;

                MyApp = new Excel.Application
                {
                    Visible = false
                };

                MyBook = MyApp.Workbooks.Open(path);
                MySheet = MyBook.Sheets[1];
                int lastRow = MySheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Row;

                int added = 0;

                for (int i = 1; i <= lastRow; i++)
                {
                    if (MySheet.Cells[i, 1].value != null)
                    {
                        string item = (MySheet.Cells[i, 1].value).ToString();

                        if (item.Contains(";"))
                        {
                            list = item.Split(';').ToList();

                            for (int j = 0; j < list.Count; j++)
                            {
                                added++;
                                EmailList.Items.Add(list[j]);
                            }

                            list.Clear();
                        }
                        else
                        {
                            EmailList.Items.Add(item);
                            added++;
                        }
                    }
                }

                Logs.Items.Add("Imported " + added + " E-mails");
            }
            else
                Logs.Items.Add("Failed");

        }

        private void Attachment1_Click(object sender, RoutedEventArgs e)
        {
            Logs.Items.Add("Adding attachment...");

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Attachment1_path = dlg.FileName;
                path1Text.Name = "Added";
                Logs.Items.Add("Added " + dlg.SafeFileName);

            }
            else
                Logs.Items.Add("Failed...");
        }

        private void Attachment2_Click(object sender, RoutedEventArgs e)
        {
            Logs.Items.Add("Adding attachment...");

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Attachment2_path = dlg.FileName;
                path2Text.Name = "Added";
                Logs.Items.Add("Added " + dlg.SafeFileName);

            }
            else
                Logs.Items.Add("Failed...");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Attachment2_path = null;
            path2Text.Name = "Add";

            Attachment1_path = null;
            path1Text.Name = "Add";
        }
    }
}
