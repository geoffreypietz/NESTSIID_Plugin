using System;
using System.Text;
using System.Web;
using Scheduler;
using HSPI_NESTSIID.Models;


namespace HSPI_NESTSIID
{
    public class OptionsPage : PageBuilderAndMenu.clsPageBuilder
    {
        public string pin_code;

        public OptionsPage(string pagename) : base(pagename)
        {

        }

        public override string postBackProc(string page, string data, string user, int userRights)
        {

            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);

            Console.WriteLine(data);

            string id = parts["id"];
            if(parts["pin_code"] != null)
            {
                pin_code = parts["pin_code"];
                Console.WriteLine(pin_code);
            }
            if (parts["id"] != null && parts["id"].Contains("access_button"))
            {
                using (var nest = new NestConnection())
                {
                    if (nest.retrieveAccess(pin_code))
                    {
                        pageCommands.Add("popmessage", "Successfully reset Access Token");
                    }
                    else
                    {
                        pageCommands.Add("popmessage", "Failed to reset Access Token");
                    } 
                }
            }        

            return base.postBackProc(page, data, user, userRights);
        }




        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder pluginSB = new StringBuilder();
            OptionsPage page = this;

            try
            {
                page.reset();

                // handle queries with special data
                /*System.Collections.Specialized.NameValueCollection parts = null;
                if ((!string.IsNullOrEmpty(queryString)))
                {
                    parts = HttpUtility.ParseQueryString(queryString);
                }
                if (parts != null)
                {
                    if (parts["myslide1"] == "myslide_name_open")
                    {
                        // handle a get for tab content
                        string name = parts["name"];
                        return ("<table><tr><td>cell1</td><td>cell2</td></tr><tr><td>cell row 2</td><td>cell 2 row 2</td></tr></table>");
                        //Return ("<div><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br></div>")
                    }
                    if (parts["myslide1"] == "myslide_name_close")
                    {
                        return "";
                    }
                }*/

                this.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME, "", "", false, true));
                //pluginSB.Append("<link rel = 'stylesheet' href = 'hspi_nestsiid/css/style.css' type = 'text/css' /><br>");
                //page.AddHeader(pluginSB.ToString());



                //page.RefreshIntervalMilliSeconds = 5000
                // handler for our status div
                //stb.Append(page.AddAjaxHandler("/devicestatus?ref=3576", "stat"))
                pluginSB.Append(this.AddAjaxHandlerPost("action=updatetime", this.PageName));



                // page body starts here




                //pluginSB.Append(clsPageBuilder.DivStart("pluginpage", ""));
                //Dim dv As DeviceClass = GetDeviceByRef(3576)
                //Dim CS As CAPIStatus
                //CS = dv.GetStatus

                pluginSB.AppendLine("<table class='full_width_table' cellspacing='0' width='100%' >");
                pluginSB.AppendLine("<tr><td  colspan='1' >");
                // Status/Options Tabs

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("optionsTab", this.PageName);





                // Options Tab
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab = new clsJQuery.Tab();
                tab.tabTitle = "Options";
                tab.tabDIVID = "nestsiid-options";

                var optionsString = new StringBuilder();
                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform1", "testpage", "post"));
                optionsString.Append("<table>");
                optionsString.Append("<tr><td class='tableheader' colspan='2'>Follow the inputs below to retrieve/reset your API access token</td></tr>");

                // Nest API Access Token
                optionsString.Append("<tr><td class='tableheader' colspan='2'>Nest API Access Token</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Follow the Retrieve Pin-Code link to obtain a pin-code</td>");
                optionsString.Append("<td class='tablecell'>");
                optionsString = BuildLinkButton(optionsString, "pin_button", "Retrieve Pin-Code", "https://home.nest.com/login/oauth2?client_id=7ba01588-498d-4f20-a524-14c3c8f9134a&state=STATE");
                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Copy the Pin-Code into this box</td>");
                optionsString.Append("<td class='tablecell'>");
                optionsString = BuildTextBox(optionsString, "pin_code", "Pin-Code", "Pin-Code", "", 200);
                optionsString.Append("</td></tr>");

                optionsString.Append("<tr><td class='tablecell'>Hit the Reset button to renew your access token</td>");
                optionsString.Append("<td class='tablecell'>");
                optionsString = BuildLinkButton(optionsString, "access_button", "Reset Access-Token", "");
                optionsString.Append("</td></tr>");
               
                optionsString.Append("</table><br>");

                optionsString.Append("<p>If the Reset was successful, restart the plugin and the new token should connect.</p>");

                optionsString.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

                tab.tabContent = optionsString.ToString();
                jqtabs.tabs.Add(tab);

                pluginSB.Append(jqtabs.Build());
                pluginSB.AppendLine("</td></tr></table>");
            }
            catch (Exception ex)
            {
                pluginSB.Append("Status/Options error: " + ex.Message);
            }
            pluginSB.Append("<br>");

            pluginSB.Append(DivEnd());
            page.AddBody(pluginSB.ToString());

            return page.BuildPage();
        }

        // Builds input textboxs for client-id, client-secret, pin-code, access-token
        private StringBuilder BuildTextBox(StringBuilder optionsString, string name, string prompt, string tooltip, string initial, int width)
        {
            clsJQuery.jqTextBox tokenTextBox = new clsJQuery.jqTextBox(name, "text", initial, this.PageName, 30, true);
            tokenTextBox.promptText = prompt;
            tokenTextBox.toolTip = tooltip;
            tokenTextBox.dialogWidth = width;
            optionsString.Append(tokenTextBox.Build());

            return optionsString;
        }

        private StringBuilder BuildHelpButton(StringBuilder optionsString, string oName, string oTooltip, string oLabel, string oText, string bName, string bLabel, string bUrl)
        {
            clsJQuery.jqOverlay ol = new clsJQuery.jqOverlay(oName, this.PageName, false, "events_overlay");
            ol.toolTip = oTooltip;
            ol.label = oLabel;
            clsJQuery.jqButton button = new clsJQuery.jqButton(bName, bLabel, this.PageName, true);
            button.url = bUrl;

            ol.overlayHTML = PageBuilderAndMenu.clsPageBuilder.FormStart("overlayformm", "testpage", "post");
            ol.overlayHTML += "<div>" + oText + "<br><br>" + button.Build() + "</div>";
            ol.overlayHTML += PageBuilderAndMenu.clsPageBuilder.FormEnd();
            optionsString.Append(ol.Build());

            return optionsString;
        }

        private StringBuilder BuildLinkButton(StringBuilder optionsString, string bName, string bLabel, string bUrl)
        {
            clsJQuery.jqButton button = new clsJQuery.jqButton(bName, bLabel, this.PageName, true);
            button.url = bUrl;
            optionsString.Append(button.Build());

            return optionsString;
        }
    }

}
