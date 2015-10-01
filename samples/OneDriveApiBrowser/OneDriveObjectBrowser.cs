using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.OneDrive.Sdk;

namespace OneDriveApiBrowser
{

    public partial class OneDriveObjectBrowser : UserControl
    {
        private Item _item;
        private PropertyDisplayFormat _format;
        private readonly Serializer serializer;

        public OneDriveObjectBrowser()
        {
            this.serializer = new Serializer();

            InitializeComponent();
            comboBoxPropertyFormat.SelectedIndex = (int)this.DisplayFormat;
            PropertyDisplayFormatChanged();
        }

        public Item SelectedItem
        {
            get
            {
                return _item;
            }
            set
            {
                if (value == _item) return;
                _item = value;
                SelectedItemChanged();
            }
        }

        [System.ComponentModel.Browsable(true)]
        public PropertyDisplayFormat DisplayFormat
        {
            get { return _format; }
            set
            {
                if (value == _format) return;
                _format = value;
                comboBoxPropertyFormat.SelectedIndex = (int)_format;
                PropertyDisplayFormatChanged();
            }
        }

        private void SelectedItemChanged()
        {
            BuildPropertyUI(this.SelectedItem);
        }

        private void PropertyDisplayFormatChanged()
        {
            switch (this.DisplayFormat)
            {
                case OneDriveApiBrowser.PropertyDisplayFormat.RawJson:
                    treeViewProperties.Visible = false;
                    propertyGridBrowser.Visible = false;
                    textBoxRawJson.Visible = true;
                    textBoxRawJson.Dock = DockStyle.Fill;
                    break;

                case OneDriveApiBrowser.PropertyDisplayFormat.TreeNode:
                    textBoxRawJson.Visible = false;
                    propertyGridBrowser.Visible = false;
                    treeViewProperties.Visible = true;
                    treeViewProperties.Dock = DockStyle.Fill;
                    break;

                case OneDriveApiBrowser.PropertyDisplayFormat.ObjectBrowser:
                    treeViewProperties.Visible = false;
                    textBoxRawJson.Visible = false;
                    propertyGridBrowser.Visible = true;
                    propertyGridBrowser.Dock = DockStyle.Fill;
                    break;
                default:
                    throw new NotImplementedException();
            }

            BuildPropertyUI(this.SelectedItem);
        }

        private void BuildPropertyUI(Item item)
        {
            if (null == item) return;

            switch (this.DisplayFormat)
            {
                case OneDriveApiBrowser.PropertyDisplayFormat.TreeNode:
                    var propertyNodes = ObjectToTreeNodes(item);
                    treeViewProperties.Nodes.Clear();
                    treeViewProperties.Nodes.AddRange(propertyNodes.ToArray());
                    break;
                case OneDriveApiBrowser.PropertyDisplayFormat.RawJson:
                    var jsonData = this.serializer.SerializeObject(item);
                    textBoxRawJson.Text = JsonHelper.FormatJson(jsonData);
                    break;
                case OneDriveApiBrowser.PropertyDisplayFormat.ObjectBrowser:
                default:
                    propertyGridBrowser.SelectedObject = item;
                    break;
            }
        }

        private static List<TreeNode> DictionaryToTreeNodes(IDictionary<string, object> dict)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var key in dict.Keys)
            {
                object value = dict[key];

                if (value != null)
                {
                    var node = CreateNode(key, value);
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static List<TreeNode> ObjectToTreeNodes(object obj)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var property in obj.GetType().GetProperties())
            {
                // Make sure to ignore indexers during creation of these nodes
                if (property.GetIndexParameters().Any())
                {
                    continue;
                }

                object value = property.GetValue(obj);

                if (value != null)
                {
                    var node = CreateNode(property.Name, value);
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static TreeNode CreateNode(string key, object value)
        {
            TreeNode node = new TreeNode(key);

            Type t = value.GetType();
            if (t == typeof(string))
            {
                node.Text += "=\"" + value.ToString() + "\"";
            }
            else if (t == typeof(int) || t == typeof(double) || t == typeof(float) || t == typeof(Int64) || t == typeof(DateTime) || t == typeof(DateTimeOffset))
            {
                node.Text += "=" + value.ToString();
            }
            else if (typeof(IEnumerable<object>).IsAssignableFrom(t))
            {
                var counter = 0;
                foreach(var child in value as IEnumerable<object>)
                {
                    node.Nodes.Add(CreateNode(string.Format("[{0}]", counter), child));
                    counter++;
                }
            }
            else if (t == typeof(object[]))
            {
                object[] values = value as object[];
                for (int i = 0; i < values.Length; i++)
                {
                    node.Nodes.Add(CreateNode(string.Format("[{0}]", i), values[i]));
                }
            }
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(t))
            {
                var childNodes = DictionaryToTreeNodes((IDictionary<string, object>)value);
                node.Nodes.AddRange(childNodes.ToArray());
            }
            else
            {
                node.Nodes.AddRange(ObjectToTreeNodes(value).ToArray());
            }
            node.Tag = value.ToString();
            return node;
        }

        private void comboBoxPropertyFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayFormat = (PropertyDisplayFormat)((ComboBox)sender).SelectedIndex;
        }

        [System.ComponentModel.Browsable(true)]
        public override string Text
        {
            get { return labelSelectedItemProperties.Text; }
            set { labelSelectedItemProperties.Text = value; }
        }

        private void copyValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == treeViewProperties.SelectedNode) return;

            string value = treeViewProperties.SelectedNode.Tag as string;
            if (null != value)
            {
                System.Windows.Forms.Clipboard.SetText(value);
            }
        }

        private void copyRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == treeViewProperties.SelectedNode) return;

            string value = treeViewProperties.SelectedNode.Text;
            if (null != value)
            {
                System.Windows.Forms.Clipboard.SetText(value);
            }
        }

    }

    public enum PropertyDisplayFormat
    {
        RawJson = 0,
        TreeNode = 1,
        ObjectBrowser = 2
    }
}
