using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using System.IO.Packaging;
using System.IO;

//打印预览窗口
namespace NirIdentifier.Detect
{
    /// <summary>
    /// PrintPreView.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPreView : Window
    {

        public class DocumentPaginatorWrapper : DocumentPaginator
        {
            Size m_PageSize;
            Size m_Margin;
            DocumentPaginator m_Paginator;

            public DocumentPaginatorWrapper(DocumentPaginator paginator, Size pageSize, Size margin)
            {
                m_PageSize = pageSize;
                m_Margin = margin;
                m_Paginator = paginator;
                m_Paginator.PageSize = new Size(m_PageSize.Width - margin.Width * 2, m_PageSize.Height - margin.Height * 2);
            }

            Rect Move(Rect rect)
            {
                if (rect.IsEmpty)
                {
                    return rect;
                }
                else
                {
                    return new Rect(rect.Left + m_Margin.Width, rect.Top + m_Margin.Height, rect.Width, rect.Height);
                }
            }

            public override DocumentPage GetPage(int pageNumber)
            {
                DocumentPage page = m_Paginator.GetPage(pageNumber);

                // Create a wrapper visual for transformation and add extras
                ContainerVisual newpage = new ContainerVisual();

                ContainerVisual smallerPage = new ContainerVisual();
                smallerPage.Children.Add(page.Visual);
                smallerPage.Transform = new MatrixTransform(0.95, 0, 0, 0.95,
                    0.025 * page.ContentBox.Width, 0.025 * page.ContentBox.Height);

                newpage.Children.Add(smallerPage);

                newpage.Transform = new TranslateTransform(m_Margin.Width, m_Margin.Height);

                return new DocumentPage(newpage, m_PageSize, Move(page.BleedBox), Move(page.ContentBox));
            }


            public override bool IsPageCountValid
            {
                get
                {
                    return m_Paginator.IsPageCountValid;
                }
            }


            public override int PageCount
            {
                get
                {
                    return m_Paginator.PageCount;
                }
            }


            public override Size PageSize
            {
                get
                {
                    return m_Paginator.PageSize;
                }

                set
                {
                    m_Paginator.PageSize = value;
                }
            }


            public override IDocumentPaginatorSource Source
            {
                get
                {
                    return m_Paginator.Source;
                }
            }
        }
        public int SaveAsXps(string fileName)
        {
            object doc;


            FileInfo fileInfo = new FileInfo(fileName);


            using (FileStream file = fileInfo.OpenRead())
            {
                System.Windows.Markup.ParserContext context = new System.Windows.Markup.ParserContext();
                context.BaseUri = new Uri(fileInfo.FullName, UriKind.Absolute);
                doc = System.Windows.Markup.XamlReader.Load(file, context);
            }


            if (!(doc is IDocumentPaginatorSource))
            {
                Console.WriteLine("DocumentPaginatorSource expected");
                return -1;
            }

            using (Package container = Package.Open(fileName + ".xps", FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    xpsDoc.AddFixedDocumentSequence();
                    FixedDocumentSequence seq = xpsDoc.GetFixedDocumentSequence();
                    DocumentReference reference = new DocumentReference();
                    //reference.SetDocument(doc);
                    seq.References.Add(reference);
                    FixedDocument abc = new FixedDocument();
                    PageContent pagecnt = new PageContent();

                    abc.Pages.Add(pagecnt);
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    xpsDoc.AddFixedDocumentSequence();
                    FixedDocumentSequence aaa = xpsDoc.GetFixedDocumentSequence();

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;


                    // 8 inch x 6 inch, with half inch margin
                    //paginator = new DocumentPaginatorWrapper(paginator, new Size(21*96/2.54, 29.7*96/2.54), new Size(0*96/2.54, 0*96/2.54));
                    //PrintDialog ptdlg = new PrintDialog();
                    //ptdlg.ShowDialog();
                    //((IDocumentPaginatorSource)doc).DocumentPaginator.PageSize = new Size(ptdlg.PrintableAreaWidth, ptdlg.PrintableAreaHeight);
                    //paginator = new DocumentPaginatorWrapper(paginator, new Size(ptdlg.PrintableAreaWidth, ptdlg.PrintableAreaHeight), new Size(0 * 96 / 2.54, 0 * 96 / 2.54));

                    rsm.SaveAsXaml(paginator);
                    rsm.Commit();
                    xpsDoc.Close();
                    container.Close();


                    XpsDocument tempdoc = new XpsDocument(fileName + ".xps", FileAccess.Read);
                    //viewer.Document = tempdoc.GetFixedDocumentSequence();

                }
            }


            Console.WriteLine("{0} generated.", fileName + ".xps");


            return 0;
        }

        public PrintPreView(FixedDocument printDoc)
        {
            InitializeComponent();
            DocViewer.Document = printDoc;
        }
    }
}
