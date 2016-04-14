using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace QRCodes
{
    // Class initially written by Kean Walmsley 
    // Blog post
    // http://through-the-interface.typepad.com/through_the_interface/2010/09/
    // more-fun-with-qr-codes-encoding-different-types-of-data-inside-autocad.html
    // Project source (QrInput.cs)
    // http://through-the-interface.typepad.com/files/QrCodes-Project.zip

    // Custom exception class for our user cancellations

    public class CancellationException : System.Exception { }

    public static class CommandLineHelper
    {
        // Get various types of data to encode into a QR Code
        // and then call the encoder to generate the URL

        public static string GetTextForQrCode(
          Editor ed, ResultBuffer defs,
          out ResultBuffer rb
        )
        {
            string res = null;
            rb = null;
            string encType = null;

            // If we have some defaults passed in,
            // get the previous type and use if as default

            if (defs != null)
            {
                TypedValue[] tvs = defs.AsArray();
                encType = tvs[1].Value.ToString();
            }

            try
            {
                // Top level prompt

                PromptKeywordOptions pko =
                  new PromptKeywordOptions(
                    "\nType of data to encode"
                  );
                pko.AllowNone = true;
                pko.Keywords.Add("CAlendar");
                pko.Keywords.Add("COntact");
                pko.Keywords.Add("Email");
                pko.Keywords.Add("Geolocation");
                pko.Keywords.Add("Phone");
                pko.Keywords.Add("Text");
                pko.Keywords.Add("Url");
                pko.Keywords.Default =
                  String.IsNullOrEmpty(encType) ? "Text" : encType;

                PromptResult pkr =
                  ed.GetKeywords(pko);

                if (pkr.Status != PromptStatus.OK)
                    return null;

                // Depending on the keyword selected, call the
                // appropriate function

                switch (pkr.StringResult)
                {
                    case "CAlendar":
                        res = GetCalendarData(ed, defs, out rb);
                        break;
                    case "COntact":
                        res = GetContactData(ed, defs, out rb);
                        break;
                    case "Email":
                        res = GetEmailData(ed, defs, out rb);
                        break;
                    case "Geolocation":
                        res = GetGeolocationData(ed, defs, out rb);
                        break;
                    case "Phone":
                        res = GetPhoneData(ed, defs, out rb);
                        break;
                    case "Text":
                        res = GetTextData(ed, defs, out rb);
                        break;
                    case "Url":
                        res = GetUrlData(ed, defs, out rb);
                        break;
                }
            }
            catch (CancellationException) { }

            return res;
        }

        // Get the data for a calendar event and encode it

        private static string GetCalendarData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            // Title is mandatory

            object defTitle = GetDefault("Title", defs);
            string title =
              GetMandatoryString(ed, "Event title", defTitle);

            // All other fields are optional

            object defStart = GetDefault("Start", defs);
            DateTime start =
              GetMandatoryDateTime(ed, "Start date & time", defStart);

            object defEnd = GetDefault("End", defs);
            DateTime end =
              GetOptionalDateTime(ed, "End date & time", defEnd);

            object defLocation = GetDefault("Location", defs);
            string location =
              GetOptionalString(ed, "Location", defLocation);

            object defDesc = GetDefault("Description", defs);
            string desc =
              GetOptionalString(ed, "Description", defDesc);

            // Create XData to store these input values

            rb = RbEncoder.CreateCalendarRb(
              title, start, end, location, desc
            );

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeCalendar(
              title, start, end, location, desc
            );
        }

        // Get the data for a contact and encode it

        private static string GetContactData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            // Name is mandatory

            object defName = GetDefault("Name", defs);
            string name =
              GetMandatoryString(ed, "Name", defName);

            // All other fields are optional

            object defPhone = GetDefault("Phone", defs);
            string phone =
              GetOptionalString(ed, "Phone number", defPhone);

            object defEmail = GetDefault("Email", defs);
            string email =
              GetOptionalString(ed, "Email address", defEmail);

            object defAddress = GetDefault("Address", defs);
            string address =
              GetOptionalString(ed, "Address", defAddress);

            object defAddress2 = GetDefault("Address2", defs);
            string address2 =
              GetOptionalString(ed, "Address 2", defAddress2);

            object defWebsite = GetDefault("Website", defs);
            string website =
              GetOptionalString(ed, "Website", defWebsite);

            object defMemo = GetDefault("Memo", defs);
            string memo =
              GetOptionalString(ed, "Memo", defMemo);

            // Create XData to store these input values

            rb = RbEncoder.CreateContactRb(
              name, phone, email, address, address2,
              website, memo
            );

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeContact(
              name, phone, email, address,
              address2, website, memo
            );
        }

        // Get the data for an email address and encode it

        private static string GetEmailData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            object defEmail = GetDefault("EValue", defs);
            string email =
              GetMandatoryString(ed, "Email address", defEmail);

            // Create XData to store the input value

            rb = RbEncoder.CreateEmailRb(email);

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeEmail(email);
        }

        // Get the data for a geo-location and encode it

        private static string GetGeolocationData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            object defLat = GetDefault("Lat", defs);
            double lat =
              GetMandatoryDouble(ed, "Latitude", defLat);

            object defLng = GetDefault("Lat", defs);
            double lng =
              GetMandatoryDouble(ed, "Longitude", defLng);

            object defQuery = GetDefault("Query", defs);
            string query =
              GetOptionalString(ed, "Query", defQuery);

            // Create XData to store these input values

            rb = RbEncoder.CreateGeolocationRb(lat, lng, query);

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeGeolocation(lat, lng, query);
        }

        // Get the data for a phone number and encode it

        private static string GetPhoneData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            object defPhone = GetDefault("PValue", defs);
            string phone =
              GetMandatoryString(ed, "Phone number", defPhone);

            // Create XData to store the input value

            rb = RbEncoder.CreatePhoneRb(phone);

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodePhone(phone);
        }

        // Get the data for some text and encode it

        private static string GetTextData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            object defText = GetDefault("TValue", defs);
            string text = GetMandatoryString(ed, "Text", defText);

            // Create XData to store the input value

            rb = RbEncoder.CreateTextRb(text);

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeText(text);
        }

        // Get the data for a URL and encode it

        private static string GetUrlData(
          Editor ed, ResultBuffer defs, out ResultBuffer rb
        )
        {
            object defUrl = GetDefault("UValue", defs);
            string url = GetMandatoryString(ed, "Url", defUrl);

            // Create XData to store the input value

            rb = RbEncoder.CreateUrlRb(url);

            // Encode the data into a URL to generate a QR Code

            return FormatDataHelper.EncodeUrl(url);
        }

        // Parse the XData from an existing QR Code for
        // the default value we care about
        // (could be optimised with a HashSet, but this would
        // only be for "complex" data such as contacts)

        private static object GetDefault(
          string name, ResultBuffer defs
        )
        {
            if (defs != null)
            {
                TypedValue[] tvs = defs.AsArray();
                for (int i = 0; i < tvs.Length; i++)
                {
                    if (tvs[i].Value.ToString() == name)
                    {
                        return tvs[i + 1].Value;
                    }
                }
            }
            return null;
        }

        // Ask the user to enter a mandatory string field

        private static string GetMandatoryString(
          Editor ed, string prompt, object defVal
        )
        {
            PromptStringOptions pso =
              new PromptStringOptions("\n" + prompt + ": ");
            pso.AllowSpaces = true;
            if (defVal != null)
            {
                pso.DefaultValue = (string)defVal;
                pso.UseDefaultValue = true;
            }
            PromptResult pr;
            bool isEmpty;

            do
            {
                pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                    throw new CancellationException();
                isEmpty =
                  String.IsNullOrEmpty(pr.StringResult);
                if (isEmpty)
                    ed.WriteMessage("\nRequired field.");
            }
            while (isEmpty);

            return pr.StringResult;
        }

        // Ask the user to enter an optional string field

        private static string GetOptionalString(
          Editor ed, string prompt, object defVal
        )
        {
            PromptStringOptions pso =
              new PromptStringOptions(
                "\n" + prompt + " (optional): "
              );
            pso.AllowSpaces = true;
            if (defVal != null)
            {
                pso.DefaultValue = (string)defVal;
                pso.UseDefaultValue = true;
            }

            PromptResult pr = ed.GetString(pso);
            if (pr.Status != PromptStatus.OK)
                throw new CancellationException();
            return pr.StringResult;
        }

        // Ask the user to enter a mandatory double field

        private static double GetMandatoryDouble(
          Editor ed, string prompt, object defVal
        )
        {
            PromptDoubleOptions pdo =
              new PromptDoubleOptions("\n" + prompt + ": ");
            pdo.AllowNegative = true;
            pdo.AllowZero = true;
            pdo.AllowNone = false;
            if (defVal != null)
            {
                pdo.DefaultValue = (double)defVal;
                pdo.UseDefaultValue = true;
            }

            PromptDoubleResult pdr = ed.GetDouble(pdo);
            if (pdr.Status != PromptStatus.OK)
                throw new CancellationException();
            return pdr.Value;
        }
        // Ask the user to enter a mandatory date/time field

        private static DateTime GetMandatoryDateTime(
          Editor ed, string prompt, object defVal
        )
        {
            DateTime res = new DateTime();
            bool failed;

            do
            {
                PromptStringOptions pso =
                  new PromptStringOptions("\n" + prompt + ": ");
                pso.AllowSpaces = true;
                if (defVal != null)
                {
                    pso.DefaultValue = (string)defVal;
                    pso.UseDefaultValue = true;
                }

                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                    throw new CancellationException();

                failed = false;
                try
                {
                    res = DateTime.Parse(pr.StringResult);
                }
                catch
                {
                    ed.WriteMessage(
                      "\nCannot understand date/time."
                    );
                    failed = true;
                }
            }
            while (failed);

            return res;
        }

        // Ask the user to enter an optional date/time field

        private static DateTime GetOptionalDateTime(
          Editor ed, string prompt, object defVal
        )
        {
            DateTime res = new DateTime();
            bool failed;

            do
            {
                PromptStringOptions pso =
                  new PromptStringOptions(
                    "\n" + prompt + "(optional): "
                  );
                pso.AllowSpaces = true;
                if (defVal != null)
                {
                    pso.DefaultValue = (string)defVal;
                    pso.UseDefaultValue = true;
                }

                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                    throw new CancellationException();

                if (String.IsNullOrEmpty(pr.StringResult))
                    return new DateTime();

                failed = false;
                try
                {
                    res = DateTime.Parse(pr.StringResult);
                }
                catch
                {
                    ed.WriteMessage(
                      "\nCannot understand date/time. Hit return to skip."
                    );
                    failed = true;
                }
            }
            while (failed);

            return res;
        }
    }
}
