// Rule: Omit any spaces in filename when using it to create the root node.
// Rule: Ignore (do not transfer) commented lines
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;						// For StringBuilder
using System.Xml;
using System.IO;							// For FileInfo et al

namespace INI2XML
{
	/// <summary>User Interface for INI file to XML file conversion utility.</summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private const string C_STR_FILE_EXT_XML = ".xml";
		private string	m_sSourceFile,
										m_sDestFile;
		private System.Windows.Forms.Label lblINIFile;
		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.TextBox txtINIFilename;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.Button button1;
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;

		public frmMain(string[] pArgs)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			ParseCommandLine(pArgs);				// Parse the command-line to set the command-line argument variables.
		}

		/// <summary>Clean up any resources being used.</summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblINIFile = new System.Windows.Forms.Label();
			this.txtINIFilename = new System.Windows.Forms.TextBox();
			this.btnConvert = new System.Windows.Forms.Button();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblINIFile
			// 
			this.lblINIFile.Location = new System.Drawing.Point(4, 4);
			this.lblINIFile.Name = "lblINIFile";
			this.lblINIFile.Size = new System.Drawing.Size(100, 16);
			this.lblINIFile.TabIndex = 0;
			this.lblINIFile.Text = "&INI Filename";
			// 
			// txtINIFilename
			// 
			this.txtINIFilename.Location = new System.Drawing.Point(4, 20);
			this.txtINIFilename.Name = "txtINIFilename";
			this.txtINIFilename.Size = new System.Drawing.Size(288, 20);
			this.txtINIFilename.TabIndex = 2;
			this.txtINIFilename.Text = "D:\\Temp\\INIFile.ini";
			// 
			// btnConvert
			// 
			this.btnConvert.Location = new System.Drawing.Point(216, 0);
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.Size = new System.Drawing.Size(75, 20);
			this.btnConvert.TabIndex = 4;
			this.btnConvert.Text = "&Convert!";
			this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 45);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(292, 22);
			this.statusBar1.TabIndex = 5;
			this.statusBar1.Text = "Ready";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(104, 0);
			this.button1.Name = "button1";
			this.button1.TabIndex = 6;
			this.button1.Text = "button1";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 67);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.btnConvert);
			this.Controls.Add(this.txtINIFilename);
			this.Controls.Add(this.lblINIFile);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmMain";
			this.Text = "INI to XML Conversion Utility";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>The main entry point for the application.</summary>
		[STAThread]
		static void Main(string[] pArgs) 
		{
			Application.Run(new frmMain(pArgs));	// Start the main form.
		}

		/// <summary>Parse the command-line arguments into their respective variables.</summary>
		/// <param name="pArgs"></param>
		private void ParseCommandLine(string[] pArgs)
		{
			// Following 2 lines are unnecessary as we only care about one potential command-line argument right now (Source INI path and name).
//			foreach(string sArg in pArgs)
//				if(sArg.StartsWith("-") || sArg.StartsWith("/"))
			if(pArgs.Length > 0)
			{
				m_sSourceFile = pArgs[0];													// Set the source filename
				m_sDestFile = m_sSourceFile + C_STR_FILE_EXT_XML;	// Set the destination filename
				ConvertAsXML2();
				this.Close();
			}
		}

		private void btnConvert_Click(object sender, System.EventArgs e)
		{
			// Establish the source INI file and name the destination xml file from it.
			m_sSourceFile = txtINIFilename.Text;
			m_sDestFile = m_sSourceFile + C_STR_FILE_EXT_XML;
			this.ConvertAsXML2();
		}

		/// <summary>
		/// Data should be formatted as:
		/// <Filename>
		///		<Section>
		///			<Key1 value="blah"/>
		///			<Key2 value="blah"/>
		///		</Section>
		/// </Filename>
		/// </summary>
		///
		//		<test>
//			<One>
//				<id>key1</id>
//				<value>boo</value>
//			</One>
//			<One>
//				<id>key2</id>
//				<value>yah</value>
//			</One>
//			<Two>
//				<id>key1</id>
//				<value>fric</value>
//			</Two>
//			<Two>
//				<id>key2</id>
//				<value>frac</value>
//			</Two>
//		</test>
		private void ConvertAsDataSet1()
		{
			string sRootNode = String.Empty;
			// Open the INI file
			FileInfo oFile = new FileInfo(m_sSourceFile);
			if(oFile.Exists)
			{
				statusBar1.Text = "Processing INI file...";
				StreamReader oReader = new StreamReader(m_sSourceFile);

				// Use the filename (minus path and extension) as the root node name
				sRootNode = (oFile.Name.IndexOf(".") > 0) ? oFile.Name.Substring(0, oFile.Name.IndexOf(".")) : oFile.Name;
				// Make sure there are no spaces in the root node name
				sRootNode = (sRootNode.IndexOf(" ") > 0) ? sRootNode.Remove(sRootNode.IndexOf(" "), 1) : sRootNode;
				DataSet oDS = new DataSet(sRootNode);

				string sLine, sSection;
				int iTableIndex = -1;
				// Loop reading the INI file
				while((sLine = oReader.ReadLine()) != null)
				{
					if(sLine.Trim().Length > 0)
					{
						//	Catch section identifiers (convert to xml tag)
						if(sLine.StartsWith("["))	// Section name, will become an XML tag
						{
							sSection = sLine.Remove(sLine.IndexOf('['), 1);
							sSection = sSection.Remove(sSection.IndexOf(']'), 1);
							oDS.Tables.Add(sSection);
							iTableIndex++;
							oDS.Tables[iTableIndex].Columns.Add("id", typeof(string));
							oDS.Tables[iTableIndex].Columns.Add("value", typeof(string));
						}
						else
						{
							if((!sLine.StartsWith(";")) && (!sLine.StartsWith("#")))
							{
								if(sLine.IndexOf("=") > 0)
								{
									string[] aKeyValue = sLine.Split('=');
									DataRow oRow	= oDS.Tables[iTableIndex].NewRow();
									oRow["id"]		= aKeyValue[0];
									oRow["value"] = aKeyValue[1];
									oDS.Tables[iTableIndex].Rows.Add(oRow);
								}
							}
						}
					}
				}
				oReader.Close();
				// Finally:  Save the DOM document to a file
				statusBar1.Text = "Saving XML file...";
				oDS.WriteXml(oFile.FullName+".as_ds1.xml");
			}
		}

		private void ConvertAsXML1()
		{
			string sRootNode = String.Empty;
			// Open the INI file
			FileInfo oFile = new FileInfo(m_sSourceFile);
			if(oFile.Exists)
			{
				statusBar1.Text = "Processing INI file...";
				StreamReader oReader = new StreamReader(m_sSourceFile);
				XmlDocument oXML = new XmlDocument();
				XmlElement	oElem, oRootElem;
				XmlAttribute oAttr;

				// Use the filename (minus path and extension) as the root node name
				sRootNode = (oFile.Name.IndexOf(".") > 0) ? oFile.Name.Substring(0, oFile.Name.IndexOf(".")) : oFile.Name;
				// Make sure there are no spaces in the root node name
				sRootNode = (sRootNode.IndexOf(" ") > 0) ? sRootNode.Remove(sRootNode.IndexOf(" "), 1) : sRootNode;
				oElem = null;
				oRootElem = oXML.CreateElement(sRootNode);

				// Finally, append everything to the DOM document
				oXML.AppendChild(oRootElem);

				string sLine, sSection;
				// Loop reading the INI file
				while((sLine = oReader.ReadLine()) != null)
				{
					if(sLine.Trim().Length > 0)
					{
						//	Catch section identifiers (convert to xml tag)
						if(sLine.StartsWith("["))	// Section name, will become an XML tag
						{
							if(oElem != null)
							{
								oRootElem.AppendChild(oElem);
								oElem = null;
							}
							sSection = sLine.Remove(sLine.IndexOf('['), 1);
							sSection = sSection.Remove(sSection.IndexOf(']'), 1);
							oElem = oXML.CreateElement(sSection);
						}
						else
						{
							if((!sLine.StartsWith(";")) && (!sLine.StartsWith("#")))
							{
								if(sLine.IndexOf("=") > 0)
								{
									string[] aKeyValue = sLine.Split('=');
									oAttr = oXML.CreateAttribute(aKeyValue[0]);
									oAttr.Value = aKeyValue[1];
									if(oElem == null)								// Then we must have just started the file and it has key/value pairs at the top without having had a section specified first.
										oRootElem.SetAttributeNode(oAttr);
									else
										oElem.SetAttributeNode(oAttr);	// Otherwise this is the attribute of a known section.
								}
							}
						}
					}
				}
				oRootElem.AppendChild(oElem);
				oReader.Close();
				// Finally:  Save the DOM document to a file
				statusBar1.Text = "Saving XML file...";
				oXML.Save(oFile.FullName+".as_xml1.xml");
			}
		}

		/// <summary>Converts the INI file into an XML configuration file structured the way the Common Application Framework expects to find it.</summary>
		/// Structure:
		/// <Configuration>
		///		<Section>
		///			<Parameter1 value=""/>
		///			<Parameter2 value=""/>
		///		</Section>
		/// </Configuration>
		private void ConvertAsXML2()
		{
			string sRootNode = String.Empty;
			// Open the INI file
			FileInfo oFile = new FileInfo(m_sSourceFile);
			if(oFile.Exists)
			{
				statusBar1.Text = "Processing INI file...";
				StreamReader oReader = new StreamReader(m_sSourceFile);
				XmlDocument oXML = new XmlDocument();
				XmlElement	oRootElem = null,
										oElem			= null;
				XmlAttribute oAttr;

				// Use the filename (minus path and extension) as the root node name
				sRootNode = (oFile.Name.IndexOf(".") > 0) ? oFile.Name.Substring(0, oFile.Name.IndexOf(".")) : oFile.Name;
				// Make sure there are no spaces in the root node name
				sRootNode = (sRootNode.IndexOf(" ") > 0) ? sRootNode.Remove(sRootNode.IndexOf(" "), 1) : sRootNode;
				// Create an XmlElement that we'll use to append to the DOM document.
				oRootElem = oXML.CreateElement(sRootNode);

				// Finally, append everything to the DOM document
				oXML.AppendChild(oRootElem);

				string sLine, sSection;

				// Loop reading the INI file
				while((sLine = oReader.ReadLine()) != null)
				{
					if(sLine.Trim().Length > 0)
					{
						// Catch section identifiers (convert to xml tag)
						// Section name, will become an XML tag
						if(sLine.StartsWith("["))	
						{
							if(oElem != null)
							{
								oRootElem.AppendChild(oElem);
								oElem = null;
							}
							sSection = sLine.Remove(sLine.IndexOf('['), 1);
							sSection = sSection.Remove(sSection.IndexOf(']'), 1);
							oElem = oXML.CreateElement(sSection);
						}
						else
						{
							if((!sLine.StartsWith(";")) && (!sLine.StartsWith("#")))
							{
								if(sLine.IndexOf("=") > 0)
								{
									// Make sure our structure is maintained in the event that the INI file was defined as just key/value pairs with no sections defined.
									if(oElem == null)
										oElem = oXML.CreateElement("General");

									string[] aKeyValue = sLine.Split('=');
									XmlElement oKey = oXML.CreateElement(aKeyValue[0]);
									oAttr = oXML.CreateAttribute("value");
									oAttr.Value = aKeyValue[1];

									oKey.SetAttributeNode(oAttr);
									oElem.AppendChild(oKey);	// Otherwise this is the attribute of a known section.
									oRootElem.AppendChild(oElem);
								}
							}
						}
					}
				}
				oRootElem.AppendChild(oElem);
				oReader.Close();
				// Finally:  Save the DOM document to a file
				statusBar1.Text = "Saving XML file...";
				string sFilename = oFile.FullName+".as_xml2.xml";
				oXML.Save(sFilename);
				statusBar1.Text = "Saved " + sFilename;
			}
		}

		private void Test()
		{
			try
			{
				XmlDocument oXML = new XmlDocument();
				oXML.Load("test.xml");
				Console.WriteLine(oXML.InnerXml);

				XmlNodeList oFilters = oXML.SelectNodes(".//Filters");
				foreach(XmlNode oChild in oFilters)
				{
					XmlNode oNode = oChild.SelectSingleNode("Filter");
					XmlNode oBefore = oNode.SelectSingleNode("before");
					XmlNode oAfter = oNode.SelectSingleNode("after");
					XmlAttributeCollection oAtts = oNode.Attributes;
					Console.WriteLine(oAtts.GetNamedItem("name").Value);
					Console.WriteLine(oAtts.GetNamedItem("sound").Value);
					Console.WriteLine(oAtts.GetNamedItem("sendmail").Value);
					Console.WriteLine(oBefore.InnerText);
					Console.WriteLine(oAfter.InnerText);
					oBefore.InnerXml = "<![CDATA[<H2>This is a change</H2>]]>";
					oXML.Save("Test2.xml");
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("Error: " + e.Message);
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Test();
		}
	}
}
