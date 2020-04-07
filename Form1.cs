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

using Modbus.Device;
using System.IO.Ports;


//using Unme.Common;



namespace codeserveer
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
        SerialPort serial_port;
       
        
       
        public Form1()
        {
            InitializeComponent();
        }

        



        static void Connect(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 17235;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }



        
       

        //Конструктор запроса
        public void get_xml_template() {


            if (val_val_test.Count > 0)
            {

                string status = "1";
                textBox4.Text = val_val_test.Count.ToString();

                test = xml.xml_create_template(textBox1.Text, textBox2.Text, textBox3.Text,
                    val_val_test.Count.ToString(), xml.get_time(), val_val_test, status);
            }
        }

        //Соединение с сервером
        public void time_connect() {
            try
            {
                server = "127.0.0.1";
                port = 17235;
                client = new TcpClient(server, port);
            }
            catch { }
        
        }

        public void time_transfer() {
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                data = System.Text.Encoding.ASCII.GetBytes(test);
                textBox7.Text = test;
                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                textBox6.Text = responseData;
            }
            catch { }
        
        }

        public void time_disconnect() {
            try
            {
                stream.Close();
                client.Close();
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            Random r = new Random();
            
            ////Чтение данных из I-7017 ICPCON
            //read_com_data();

            //listView1.Clear();
            //add_to_tree();
            get_xml_template();
            //Передача в Сервер кодов
            time_transfer();


        }

        private void checkBox1_MouseClick(object sender, MouseEventArgs e)
        {
          
        }


        public void read_com_data(EventArgs e)
        {
            short[] AIValue = new short[8];
            short iRet;

            iRet = DCON.IO_Function.Read_AI_All_Hex(iComport, iAddress, iSlot, bChecksum, iTimeout, AIValue);

            if (Convert.ToBoolean(iRet))
            {
                checkBox2.Checked = false;
                checkBox2_Click(this, e);
                checkBox1.Checked = false;
                checkBox1_Click(this, e);
                listView1.Clear();
                MessageBox.Show("Ошибка связи: " + Convert.ToString(iRet));
               
            }


            else
            {
                //Массив значений параметров по каналам
                val_val_test.Clear();
                for (int i = 0; i < 8; i++)
                {
                    //Отсечение отрицательных кодов
                    if (AIValue[i] > 0)
                    {
                        val_val_test.Add(Convert.ToString(AIValue[i]));
                    }
                    else
                    {
                        val_val_test.Add("0");
                    }
                }
                //Добавление в дерево
                for (int i = 0; i < 8; i++)
                {
                    listView1.Items.Add("Канал " + (i + 1).ToString() + ": " + val_val_test[i]);
                }

            }
        }

        public void add_to_tree() {
          
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                timer2.Enabled = true;
               
               
            }
            else

            {
                DCON.UART.Close_Com(iComport);
                timer2.Enabled = false;
                listView1.Clear();
                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DCON.UART.Close_Com(iComport);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            listView1.Clear();

            iComport = Convert.ToByte(textComport.Text);
            iAddress = Convert.ToInt16(textAddress.Text);
            iBaudrate = Convert.ToUInt32(textBaudrate.Text);
            iSlot = Convert.ToInt16(textSlot.Text);
            bChecksum = Convert.ToInt16(textChecksum.Text);
            iTimeout = Convert.ToInt16(textTimeout.Text);
            iTotalChannel = Convert.ToInt16(textTotalChannel.Text);
            DCON.UART.Open_Com(iComport, iBaudrate, 8, 0, 1);

            //Чтение данных из I-7017 ICPCON
            read_com_data(e);
            //Добавление в список
            
            //if (val_val_test.Count > 0)
            //{
            //    add_to_tree();
            //}
        }

        public void test_read_com_data()
        {
            short[] AIValue = new short[8];
            short iRet;

            iRet = DCON.IO_Function.Read_AI_All_Hex(iComport, iAddress, iSlot, bChecksum, iTimeout, AIValue);
            


            if (Convert.ToBoolean(iRet)) { 
            
            }
            else
            {
                listView2.Items.Add("Адрес:"+iAddress);
            }
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            listView2.Clear();

            checkBox2.Checked = false;
            checkBox2_Click(this, e);
            checkBox1.Checked = false;
            checkBox1_Click(this, e);

            iComport = Convert.ToByte(textComport.Text);
            iAddress = Convert.ToInt16(textAddress.Text);
            iBaudrate = Convert.ToUInt32(textBaudrate.Text);
            iSlot = Convert.ToInt16(textSlot.Text);
            bChecksum = Convert.ToInt16(textChecksum.Text);
            iTimeout = Convert.ToInt16(textTimeout.Text);
            iTotalChannel = Convert.ToInt16(textTotalChannel.Text);

            for (Int16 i = Convert.ToInt16(textBox8.Text); i <= Convert.ToInt16(textBox9.Text); i++)
            {
                DCON.UART.Close_Com(iComport);

                iAddress = i;
                DCON.UART.Open_Com(iComport, iBaudrate, 8, 0, 1);

                test_read_com_data();

            }
        }

        //public void send_cmd_read_dcon() {

        //    byte[] out_buffer;
        //    byte[] in_buffer;
        //    UInt32  itimeo = Convert.ToUInt32(textComport.Text);
        //    UInt32 bChecksum = Convert.ToUInt32(textChecksum.Text);

        //    DCON.UART.Send_Receive_Cmd(iComport, out_buffer, in_buffer, itimeo, bChecksum, out itimeo);
        //}

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timer1.Enabled = false;
                server = textBox5.Text;

                //Создание запроса
                get_xml_template();

                //Подключение к Серверу кодов, включение таймера передачи
                time_connect();
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
                time_disconnect();

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            //timer3.Start();
            
            //int appID=99;
            //string SelfHostName = "127.0.0.1";
            //string AppIDName = "SUCK";
            //int chanCount = 5;
            //device.Identify(appID, SelfHostName, AppIDName, chanCount);
            
            //////CodeServer.Sender();

            //if (device.Connected) { timer3.Start(); }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            datas = new int[chanCount];

            for (int a = 0; a < chanCount; a++)
            {
                datas[a] = 666;
            }
            //device.
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

        //Подключение через DCOM
        public void checkBox3_Click(object sender, EventArgs e)
        {
            //Инициализация переменных
            SelfHostName = textBox5.Text;
            appID = Convert.ToInt32(textBox2.Text);
            AppIDName = textBox1.Text;
            chanCount = Convert.ToInt32(textBox4.Text);

            
            //Создание DCOM объекта
            device = new Client(SelfHostName);
            if (checkBox3.Checked)
            {  
                //Регистрация на Сервере кодов через DCOM
                device.Identify(appID, SelfHostName, AppIDName, chanCount);

                //Если соединись с Сервером кодов, запускаем передачу данных по таймеру3
                if (device.Connected) { 
                    timer3.Start(); 
                }
            }
            else {
                //Отключение!
                //device.
                timer3.Stop();
                //device = null;
                //Marshal.FinalReleaseComObject(this.);
                //device = null;
                //device.Identify(appID, "", AppIDName, 0);
                //device = new Client(SelfHostName);
                //device.Dispose();
                
                device.Dispose();
                Dispose(true);
                
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {

                //SerialPort port = new SerialPort("COM" + textBox16.Text);

                //// configure serial port
                //port.BaudRate = 57600;
                //port.DataBits = 8;
                //port.Parity = Parity.None;
                //port.StopBits = StopBits.One;
                //port.Open();

                //ModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);
                //master.Transport.ReadTimeout = 300;

                //byte slaveID = 2;
                //ushort startAddress = 2;
                //ushort numOfPoints = 1;
                //ushort[] holding_register = master.ReadHoldingRegisters(slaveID, startAddress, numOfPoints);
                //if (holding_register.Count() != 0)
                //{
                //    //string s = System.Text.Encoding.UTF8.GetString(holding_register, 0, holding_register.Count);
                //    label18.Text = holding_register[0].ToString();
                //}
                //port.Close();
                serial_port = new SerialPort("COM" + textBox16.Text);
                timer4.Interval = Convert.ToInt32(textBox12.Text);
                timer4.Start();

            }
            else {
                timer4.Stop();
                serial_port.Close();
                label18.Text = "";
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            try
            {
                //serial_port = new SerialPort("COM" + textBox16.Text);

                // configure serial port
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
                    label18.Text = holding_register[0].ToString();
                }
                else {

                    label18.Text = "Ошибка!";
                    checkBox4.Checked = false;
                    
                
                }
                master.Dispose();
                
                Application.DoEvents();
            }
            catch { label18.Text = "Ошибка!"; }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

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


