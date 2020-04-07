using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;
using DCON_PC_DOTNET;
using CodeServer;
using System.Runtime.InteropServices;
using System.Threading;
using System.Media;

using Modbus.Device;
using System.IO.Ports;


//using Unme.Common;



namespace dn130
{
    public partial class Form1 : Form
    {
        
        public xml_query xml = new xml_query();

        public string server;
        public Int32 port;
        public TcpClient client;

        //Соообщение передаваемое через сокет сервер
        public string test;
        // Массив передаваемый через сокет сервер в ASCII
        public Byte[] data ;
        public NetworkStream stream;
        public string message;

        //Список значений каналов по порядку
        public List<string> val_val_test = new List<string>();

        //i-7017
        short iSlot, iTimeout, iTotalChannel, iAddress, bChecksum;
        byte iComport;
        uint iBaudrate;

        //DCOM
        public CodeServer.Client device;
        //public CodeServer.Sender sender_ = new CodeServer.Sender();
        //public CodeServer.Constants fff=10;
        public int[] datas;
        public int appID = 99;
        public string SelfHostName = "I-7017 ICPCON";
        public string AppIDName = "SUCK";
        public int chanCount = 8;

        //MODBUS
        

        //DN-130
        //Вес на крюке значение
        public int Wkr=0;
        //Вывод ошибки
        public bool er_alarm=true;

        //Настройки
        public ini nastr = new ini();
        XmlWriterSettings settings = new XmlWriterSettings();
        XmlTextReader rdr = new XmlTextReader("Settings.xml");
       
        
       
        public Form1()
        {
            InitializeComponent();
            //foreach (TabPage _Page in tabControl1.TabPages)
            //{
            //    _Page.AutoScroll = true;
            //    _Page.AutoScrollMargin = new System.Drawing.Size(0, 20);
            //    _Page.AutoScrollMinSize = new System.Drawing.Size(_Page.Width, _Page.Height);
            //}
            //checkBox1.Text = "Если Код <=, \n\r" + "то ПОСЛЕДНЕЕ значение";
        }

        



      

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //DCON.UART.Close_Com(iComport);
            save_set();

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        

        private void timer3_Tick(object sender, EventArgs e)
        {
            //Отправка массива значений в Сервер кодов
            datas = new int[chanCount];

            for (int a = 0; a < chanCount; a++)
            {
                datas[a] = Wkr;
            }
            device.SendCodes(datas);
            //device.Dispose();
            //sender_.SendCodes(SelfHostName,appID,SelfHostName,AppIDName,chanCount,datas);
            //sender_.
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer3.Stop();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            
        }

      

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                timer4.Interval = Convert.ToInt32(textBox12.Text)+200;
                timer4.Start();
            }
            else {
                timer4.Stop();
                textBox17.Text = "";
            }
        }


        
        //public void show_alarm() {

        //    textBox17.Text = "Ошибка!";
        //    //Application.DoEvents();
        //    if (richTextBox1.Lines.Count() > 30) richTextBox1.Text = "";
        //    richTextBox1.Text += DateTime.Now + " -Ошибка ответа \n";
        //    if (checkBox8.Checked && this.WindowState != FormWindowState.Normal)
        //    {
        //        this.tabControl1.SelectedIndex = 0;
        //        this.WindowState = FormWindowState.Normal;
        //    }
        //    timer4.Stop();
        //    timer1.Interval = Convert.ToInt32(textBox1.Text);
        //    timer1.Start();
        //}

        bool firt_wav = true;
        private SoundPlayer Player = new SoundPlayer();
        //Звук
        public void sound_alarm(){

            try
            {
                // Note: You may need to change the location specified based on
                // the sounds loaded on your computer.
                this.Player.SoundLocation = @"sid.wav";
                this.Player.PlayLooping();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error playing sound");
            }
        }
        //Вывод сигнализации
        public void show_alarm(string messa)
        {
            if (firt_wav) {
                sound_alarm();
                firt_wav = false;
            }
            string mess = messa;
            textBox17.Text = "Ошибка!";
            //Application.DoEvents();
            if (richTextBox1.Lines.Count() > 30) richTextBox1.Text = "";
            richTextBox1.Text += DateTime.Now +"  "+ mess+"\n";
            if (checkBox8.Checked && this.WindowState != FormWindowState.Normal)
            {
                this.tabControl1.SelectedIndex = 0;
                this.WindowState = FormWindowState.Normal;
            }


            timer4.Stop();
            timer1.Interval = Convert.ToInt32(textBox1.Text);
            timer1.Start();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            try
            {
                SerialPort serial_port = new SerialPort("COM" + textBox16.Text);
                serial_port.BaudRate = 57600;
                serial_port.DataBits = 8;
                serial_port.Parity = Parity.None;
                serial_port.StopBits = StopBits.One;
                serial_port.Open();

                ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(serial_port);
                master.Transport.ReadTimeout = Convert.ToInt32(textBox12.Text);
                //master.Transport.WriteTimeout= Convert.ToInt32()

                byte slaveID = 2;
                ushort startAddress = 0;
                ushort numOfPoints = 1;
                ushort[] holding_register = master.ReadHoldingRegisters(slaveID, startAddress, numOfPoints);
                if (holding_register.Count() != 0)
                {
                    //string s = System.Text.Encoding.UTF8.GetString(holding_register, 0, holding_register.Count);
                    textBox17.Text = holding_register[0].ToString();
                    //Передача нового значения
                    if ((checkBox1.Checked == true) & (holding_register[0] <= (Convert.ToInt32(textBox2.Text))))
                    {
                        show_alarm("Отсечка кода:" + holding_register[0]);
                    }
                    else {
                        Wkr = holding_register[0];
                    }
                    firt_wav = true;
                    try
                    {
                        this.Player.Stop();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error playing sound");
                    }
                }
                else {
                    //Если Передача последнего значения, при ошибке ==TRUE
                    if (checkBox7.Checked==true)
                    {
                        show_alarm("Ошибка ответа");
                    }
                    else { 
                        Wkr = 0;
                        show_alarm("Ошибка ответа");
                    }
                    
                
                }
                master.Dispose();
                serial_port.Close();
                
                //Application.DoEvents();
            }
            catch {
                if (checkBox7.Checked == true)
                {
                    show_alarm("Ошибка передачи");
                }
                else { 
                    
                    Wkr = 0;
                    show_alarm("Ошибка передачи");                
                }
            
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }

        //Подключение по DCOM к DT-CIS
        private void checkBox5_Click(object sender, EventArgs e)
        {
            
            if (checkBox5.Checked)
            {
                //Инициализация переменных
                SelfHostName = textBox10.Text;
                appID = Convert.ToInt32(textBox14.Text);
                AppIDName = textBox15.Text;
                chanCount = 1;


                //Создание DCOM объекта
                device = new Client(SelfHostName);
                //Регистрация на Сервере кодов через DCOM
                device.Identify(appID, SelfHostName, AppIDName, chanCount);

                //Если соединись с Сервером кодов, запускаем передачу данных по таймеру3
                if (device.Connected)
                {
                    timer3.Start();
                }
            }
            else
            {
                //Отключение!
                timer3.Stop();
                device.Dispose();
                Application.Exit();

            }
        }


        //Cохранение настроек
        public void save_set()
        {
            try
            {
                nastr.com_port = textBox16.Text;
                nastr.timeout = textBox12.Text;
                nastr.old_value = checkBox7.Checked.ToString();
                nastr.show_error = checkBox8.Checked.ToString();
                nastr.ip_server_code = textBox10.Text;
                nastr.name_device = textBox15.Text;
                nastr.id_device = textBox14.Text;
                nastr.timeout_repeat = textBox1.Text;
                nastr.code_cut_off = textBox2.Text;
                nastr.code_cut_off_on = checkBox1.Checked.ToString();

                settings.Indent = true;
                settings.IndentChars = ("    ");
                using (XmlWriter writer = XmlWriter.Create("Settings.xml", settings))
                {
                    // Write XML time.
                    writer.WriteStartElement(nastr.Sect);
                    writer.WriteElementString("com_port", nastr.com_port);
                    writer.WriteElementString("timeout", nastr.timeout);
                    writer.WriteElementString("old_value", nastr.old_value);
                    writer.WriteElementString("show_error", nastr.show_error);
                    writer.WriteElementString("ip_server_code", nastr.ip_server_code);
                    writer.WriteElementString("name_device", nastr.name_device);
                    writer.WriteElementString("id_device", nastr.id_device);
                    writer.WriteElementString("timeout_repeat", nastr.timeout_repeat);
                    writer.WriteElementString("code_cut_off", nastr.code_cut_off);
                    writer.WriteElementString("code_cut_off_on", nastr.code_cut_off_on);
                    writer.WriteEndElement();
                    writer.Flush();
                    writer.Close();
                }
            }
            catch { }
        }

        //Чтение настроек
        public void read_set()
        {
            try
            {

                ////ini файл

                while (rdr.Read())
                {

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "com_port")
                    {
                        rdr.Read();
                        textBox16.Text = rdr.Value.ToString();

                    }
                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "timeout")
                    {
                        rdr.Read();
                        textBox12.Text = rdr.Value.ToString();

                    }
                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "old_value")
                    {
                        rdr.Read();
                        checkBox7.Checked = Convert.ToBoolean(rdr.Value);
                        //comboBox1.SelectedItem = comboBox1.Text;
                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "show_error")
                    {
                        rdr.Read();
                        checkBox8.Checked = Convert.ToBoolean(rdr.Value);
                        //comboBox1.SelectedItem = comboBox1.Text;
                    }
                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "ip_server_code")
                    {
                        rdr.Read();
                        textBox10.Text = rdr.Value.ToString();

                    }
                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "name_device")
                    {
                        rdr.Read();
                        textBox15.Text = rdr.Value.ToString();

                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "id_device")
                    {
                        rdr.Read();
                        textBox14.Text = rdr.Value.ToString();

                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "timeout_repeat")
                    {
                        rdr.Read();
                        textBox1.Text = rdr.Value.ToString();

                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "code_cut_off")
                    {
                        rdr.Read();
                        textBox2.Text = rdr.Value.ToString();

                    }

                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "code_cut_off_on")
                    {
                        rdr.Read();
                        checkBox1.Checked = Convert.ToBoolean(rdr.Value);
                    }
                }
                rdr.Close();
            }
            catch { }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            read_set();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal) {
                this.tabControl1.SelectedIndex = 0;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer4.Start();
            timer1.Stop();
        }

    }
}

//Send_Receive_Cmd 
//This  function  sends  a  command  string  to  RS485  Network  and  receives  the  
//response from RS485 Network. If the wCheckSum=1, this function automatically adds the two checksum bytes into the command string and also check the checksum status when  receiving  response  from  the  modules.  Note  that  the  end  of  sending  string  is  added [0x0D] to mean the termination of every command.
//Syntax: 
//Send_Receive_Cmd   (char   cPort,   char   szCmd[],   char   szResult[],   WORD wTimeOut, WORD wCheckSum, WORD *wT) 
//Input Parameter: 
//cPort:  1=COM1,       2=COM2,       3=COM3, 4=COM4 .... , 255=COM255 
//szCmd:   Sending       command       string       
//szResult:   Receiving the response string from the modules 
//wTimeOut:    Communicating  timeout setting, time unit = 1ms 
//wCheckSum:   0=DISABLE,  1=ENABLE  
//*wT:   Total time of send/receive interval, unit = 1 ms 
//Return Value:  NoError : OK 
//Others    :     Error       code       Others       :                     Error       code       


//[7017]
//DI_CHANNEL_COUNT=0
//DO_CHANNEL_COUNT=0
//AI_CHANNEL_COUNT=8
//AO_CHANNEL_COUNT=0
//COUNT_FREQ_CHANNEL_COUNT=0
//DESCRIPTION=8*AI (mA,mV,V)
//FORMDLL=UI_VC
//NAMESPACE=ICPDAS
//CLASS=AI_VC
//LANGUAGE=SAVED

//[7052]
//DI_CHANNEL_COUNT=8
//DO_CHANNEL_COUNT=0
//AI_CHANNEL_COUNT=0
//AO_CHANNEL_COUNT=0
//COUNT_FREQ_CHANNEL_COUNT=0
//DESCRIPTION=8*DI 
//FORMDLL=UI_DIO
//NAMESPACE=ICPDAS
//CLASS=DIO

//[7080]
//DI_CHANNEL_COUNT=0
//DO_CHANNEL_COUNT=2
//AI_CHANNEL_COUNT=0
//AO_CHANNEL_COUNT=0
//COUNT_FREQ_CHANNEL_COUNT=2
//DESCRIPTION=2*Counter/Frequency + 2*DO
//FORMDLL=UI_CntFreq
//NAMESPACE=ICPDAS
//CLASS=CounterFreq
//LANGUAGE=SAVED


//В программу «Сервер Кодов» версия 1-70(314) добавлен сервер приема данных от драйверов по протоколу TCP (сокет-сервер) порт 17235.

//На первые символы // не обращай внимания, это сишный комментарий.
//Текст заключенный <!-- … --> это XML-комментарий, который тоже не надо вставлять в пакет, так как это просто мое пояснение. Для меня не важно оформление и сдвиги как в XML, я все это проглочу, важны начало и конец каждого блока или значения, а также всего пакета. То есть можно писать все в одну строчку или каждый блок на новой строчке со сдвигами.
//<..> - начало блока или значения
//</…> - конец блока или значения

//Пакет Данных в кодовой таблице WINDOWS от клиента:

//  <driver>  - начало пакета
//
//      <!-- Блок информации о драйвере или устройстве, выводимое в окне Сервера Кодов.
//           Блок высылается один раз после установки соединения и при изменении
//           параметров устройства. Без этой информации поступающие данные будут
//           игнорироваться. -->
//      <info>  - начало блока
//          <id>123</id> - идентификатор хранения настроек привязки каналов в сервере (больше 0)
//          <name>Имя драйвера</name>     - наименование
//          <maxchannels>32</maxchannels> - количество каналов (больше 0)
//      </info> - конец блока
//
//      <!-- Блок данных каналов, поступающих от устройства. Список каналов находится
//           в интервале 1...<maxchannels> и может быть не полным, то есть меньше <maxchannels>.
//           - Значение канала <value> может быть только целым числом.
//           - Код состояния канала <status>:
//              0 - ошибка по каналу, значение неопределено
//              1 - корректное значение канала в цифровых кодах или др.величине
//              2 - значение после корректировки канала
//              3 - корректное значение канала в микровольтах
//              4 - корректное значение канала в микроамперах
//           - Дата блока данных <datetime> используется, если устройство является
//             синхронизирующим. -->
//      <data>  - начало блока
//          <datetime>YYYYMMDDHHNNSSsss</datetime> - дата и время блока данных
//          <channel>  - начало блока данных канала
//              <number>1</number>        - номер канала в драйвере
//              <value>1234567890</value> - значение канала
//              <status>1</status>        - состояние значения канала
//          </channel> - конец блока данных канала
//          ...
//      </data> - конец блока
//
//  </driver> - конец пакета

//Например:
//<driver>
//     <info>
//         <id>123</id>
//         <name>Хроматограф СГА-05</name>
//         <maxchannels>8</maxchannels>
//     </info>
//     <data>
//         <datetime>20070205114152200</datetime>   - это время 5.2.2007 11:41:52.200
//         <channel>
//             <number>1</number>
//             <value>1234567890</value>
//             <status>1</status>
//         </channel>
//         <channel>
//             <number>2</number>
//             <value>12345</value>
//             <status>1</status>
//         </channel>
//         <channel>
//             <number>5</number>
//             <value>67890</value>
//             <status>1</status>
//         </channel>
//     </data>
//</driver>

//На каждый такой пакет, распознанный сервером, сервер отвечает результатом операции в виде:

//  <driver>  - начало пакета
//
//      <!-- Блок возвращаемого результата. Блок возвращается клиенту на каждый пакет данных.
//           Список кодов ошибок:
//               больше 0 - успешно. Код возвращается только при приеме блока информации
//                   о драйвере или устройстве. Значение содержит максимальное количество
//                   каналов, которое Сервер Кодов смог выделить для данного устройства.
//               0 - успешно
//              -1 - нет места для размещения данных устройства
//              -2 - ошибка открытия Базы кодов, регистрация данных невозможна
//              -3 - неверная регистрационная информация
//              -4 - дублирование идентификатора. Драйвер с таким идентификатором уже
//                   присоединен к Серверу Кодов -->
//      <result>  - начало блока
//          <code>0</code>              - код выполнения операции
//          <error>Текст ошибки</error> - текст ошибки (необязательно)
//      </result> - конец блока
//
//  </driver> - конец пакета

//Например:
//<driver>
//     <result>
//         <code>0</code>
//         <error>Текст ошибки</error>
//     </result>
//</driver>


