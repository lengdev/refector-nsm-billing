using System;
using System.Text;

namespace MVC
{
    public class HtmlDateTimePickerFromDateToDate : HTMLControl
    {

        public bool DefaultValueDate = true;
        public string FromDateID = "TxtFromDate";
        public string FromDateValue = "";
        public string LabelFromDate = "From Date:";
        public string LabelToDate = "To Date:";
        public string ToDateID = "TxtToDate";
        public string ToDateValue = "";
        public bool ToDay = false;
        public HtmlDateTimePickerFromDateToDate()
        {
        }

        public override string GetHtml()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            StringBuilder stb_result = new StringBuilder();
            if (DateRangePicker)
            {
                stb_result.AppendLine("<tr><td align='" + LabelAlign + "' valign='" + LabelValign + "'><label for=\"" + FromDateID + "\" class=\"col-form-label\">" + LabelFromDate + "</label></td>");
                stb_result.AppendLine("<td><input type=\"text\" name='" + FromDateID + "' id='" + FromDateID + "' value='" + Value + "' class='" + Class + "' placeholder=\"" + placeholder + "\" />");
                stb_result.AppendLine("</td></tr>");
                stb_result.AppendLine("<script type=\"text/javascript\">");
                stb_result.AppendLine("$(function() { ");
                if (DateTimePickerWithRanges || DateRangePicker)
                {
                    if (ToDay)
                        stb_result.AppendLine("let start = moment(); let end = moment();");
                    else
                        stb_result.AppendLine("let start = moment().startOf('month'); let end = moment();");

                    if (!string.IsNullOrEmpty(FromDateValue))
                    {
                        stb_result.AppendLine("start = moment(`" + FromDateValue + "`, 'YYYY-MM-DD');");
                    }
                    stb_result.AppendLine("function cb(start, end, label) {");
                    if (!SingleDatePicker)
                        stb_result.AppendLine("$('#" + FromDateID + "').val(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));");
                    stb_result.AppendLine("}");
                    stb_result.AppendLine("$('#" + FromDateID + "').daterangepicker({");
                    stb_result.AppendLine("alwaysShowCalendars: true,");
                    stb_result.AppendLine("showDropdowns: true, showWeekNumbers: true, showISOWeekNumbers: true,");
                    stb_result.AppendLine("autoApply: true,");
                    stb_result.AppendLine("autoUpdateInput: false,");
                    stb_result.AppendLine("locale: { format: 'DD/MM/YYYY', cancelLabel: 'Clear' },");
                    if (SingleDatePicker)
                    {
                        stb_result.AppendLine("singleDatePicker: true,");
                    }
                    if (DefaultValueDate)
                    {
                        stb_result.AppendLine("startDate: start,");
                        stb_result.AppendLine("endDate: end,");
                    }
                    if (!SingleDatePicker)
                    {
                        stb_result.AppendLine("ranges: {");
                        stb_result.AppendLine("'Today': [moment(), moment()],");
                        stb_result.AppendLine("'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],");
                        stb_result.AppendLine("'Last 7 Days': [moment().subtract(6, 'days'), moment()],");
                        stb_result.AppendLine("'Last 30 Days': [moment().subtract(29, 'days'), moment()],");
                        stb_result.AppendLine("'This Month': [moment().startOf('month'), moment().endOf('month')],");
                        stb_result.AppendLine("'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]");
                        stb_result.Append("}}, cb).on('apply.daterangepicker', function (ev, picker) {");
                    }
                    else
                        stb_result.Append("}).on('apply.daterangepicker', function (ev, picker) {");
                    stb_result.AppendLine("start = picker.startDate.format('DD/MM/YYYY');");
                    stb_result.AppendLine("end = picker.endDate.format('DD/MM/YYYY');");
                    if (!SingleDatePicker)
                    {
                        stb_result.AppendLine("$('#" + FromDateID + "').val(start + ' - ' + end);");
                        stb_result.Append("if(picker.chosenLabel == 'Today') $('#" + FromDateID + "').val(moment().format('DD/MM/YYYY') + ' - ' + moment().format('DD/MM/YYYY'));})");
                    }
                    else
                    {
                        stb_result.AppendLine("$('#" + FromDateID + "').val(start);");
                        stb_result.Append("if(picker.chosenLabel == 'Today') $('#" + FromDateID + "').val(moment().format('DD/MM/YYYY'));})");
                    }
                    stb_result.Append(".on('cancel.daterangepicker', function(ev, picker) {\r\n$(this).val('');");
                    stb_result.Append("}).val('');");
                    if (DefaultValueDate)
                        stb_result.AppendLine("cb(start, end);");

                    // --- Custom-styled month/year dropdowns for the calendar (registers once globally) ---
                    stb_result.AppendLine("if (typeof window.__dpSelectInit === 'undefined') {");
                    stb_result.AppendLine("window.__dpSelectInit = true;");
                    stb_result.AppendLine("function dpBuildSelect(selectEl) {");
                    stb_result.AppendLine("  if (selectEl.getAttribute('data-dp-done') === '1') return;");
                    stb_result.AppendLine("  selectEl.setAttribute('data-dp-done', '1');");
                    stb_result.AppendLine("  selectEl.style.display = 'none';");
                    stb_result.AppendLine("  var wrap = document.createElement('div');");
                    stb_result.AppendLine("  wrap.className = 'dp-select';");
                    stb_result.AppendLine("  var trigger = document.createElement('button');");
                    stb_result.AppendLine("  trigger.type = 'button';");
                    stb_result.AppendLine("  trigger.className = 'dp-select-trigger';");
                    stb_result.AppendLine("  trigger.innerHTML = '<span>' + selectEl.options[selectEl.selectedIndex].text + '</span><i>&#9662;</i>';");
                    stb_result.AppendLine("  var list = document.createElement('ul');");
                    stb_result.AppendLine("  list.className = 'dp-select-list';");
                    stb_result.AppendLine("  Array.prototype.forEach.call(selectEl.options, function (opt) {");
                    stb_result.AppendLine("    var li = document.createElement('li');");
                    stb_result.AppendLine("    li.textContent = opt.text;");
                    stb_result.AppendLine("    li.className = 'dp-select-item' + (opt.selected ? ' selected' : '');");
                    stb_result.AppendLine("    li.addEventListener('click', function (e) {");
                    stb_result.AppendLine("      e.stopPropagation();");
                    stb_result.AppendLine("      selectEl.value = opt.value;");
                    stb_result.AppendLine("      $(selectEl).trigger('change');");
                    stb_result.AppendLine("      wrap.classList.remove('open');");
                    stb_result.AppendLine("    });");
                    stb_result.AppendLine("    list.appendChild(li);");
                    stb_result.AppendLine("  });");
                    stb_result.AppendLine("  trigger.addEventListener('click', function (e) {");
                    stb_result.AppendLine("    e.preventDefault();");
                    stb_result.AppendLine("    e.stopPropagation();");
                    stb_result.AppendLine("    document.querySelectorAll('.dp-select.open').forEach(function (w) { if (w !== wrap) w.classList.remove('open'); });");
                    stb_result.AppendLine("    wrap.classList.toggle('open');");
                    stb_result.AppendLine("    if (wrap.classList.contains('open')) {");
                    stb_result.AppendLine("      var sel = list.querySelector('.dp-select-item.selected');");
                    stb_result.AppendLine("      if (sel) {");
                    stb_result.AppendLine("        list.scrollTop = sel.offsetTop - (list.clientHeight / 2) + (sel.offsetHeight / 2);");
                    stb_result.AppendLine("      }");
                    stb_result.AppendLine("    }");
                    stb_result.AppendLine("  });");
                    stb_result.AppendLine("  wrap.appendChild(trigger);");
                    stb_result.AppendLine("  wrap.appendChild(list);");
                    stb_result.AppendLine("  selectEl.parentNode.insertBefore(wrap, selectEl.nextSibling);");
                    stb_result.AppendLine("}");
                    stb_result.AppendLine("function dpBuildAll(container) {");
                    stb_result.AppendLine("  container.querySelectorAll('select.monthselect:not([data-dp-done]), select.yearselect:not([data-dp-done])').forEach(dpBuildSelect);");
                    stb_result.AppendLine("}");
                    stb_result.AppendLine("document.addEventListener('click', function () {");
                    stb_result.AppendLine("  document.querySelectorAll('.dp-select.open').forEach(function (w) { w.classList.remove('open'); });");
                    stb_result.AppendLine("});");
                    stb_result.AppendLine("$(document).on('show.daterangepicker', function (ev, picker) {");
                    stb_result.AppendLine("  dpBuildAll(picker.container[0]);");
                    stb_result.AppendLine("  var observer = new MutationObserver(function () { dpBuildAll(picker.container[0]); });");
                    stb_result.AppendLine("  observer.observe(picker.container[0], { childList: true, subtree: true });");
                    stb_result.AppendLine("  $(document).one('hide.daterangepicker', function () { observer.disconnect(); });");
                    stb_result.AppendLine("});");
                    stb_result.AppendLine("}");
                    // --- end custom dropdowns ---
                }
                stb_result.AppendLine("});");
                stb_result.AppendLine("</script>");
            }
            else if (!DateRangePicker)
            {
                placeholder = "Select Date";

                string[] labelAlign = new string[] { "<tr><td align='", base.LabelAlign, "' valign='", base.LabelValign, "'><label for=\"" + FromDateID + "\" class=\"col-form-label\">", this.LabelFromDate, "</label></td>" };
                stb_result.Append(string.Concat(labelAlign));
                labelAlign = new string[] { "<td><input type=\"text\" name='", this.FromDateID, "' id='", this.FromDateID, "' value='", base.Value, "' class='", base.Class, "' placeholder=\"" + placeholder + "\"/>" };
                stb_result.Append(string.Concat(labelAlign));
                stb_result.Append("</td></tr>");
                labelAlign = new string[] { "<tr><td align='", base.LabelAlign, "' valign='", base.LabelValign, "'><label for=\"" + ToDateID + "\" class=\"col-form-label\">", this.LabelToDate, "</label></td>" };
                stb_result.Append(string.Concat(labelAlign));
                labelAlign = new string[] { "<td><input type=\"text\" name='", this.ToDateID, "' id='", this.ToDateID, "' value='", base.Value, "' class='", base.Class, "' placeholder=\"" + placeholder + "\"/>" };
                stb_result.Append(string.Concat(labelAlign));
                stb_result.Append("</td></tr>");
                stb_result.Append("<script type=\"text/javascript\">");

                stb_result.AppendLine("$(document).ready(function() {");
                stb_result.AppendLine(string.Concat(" $(\"#", FromDateID, "\").datetimepicker({"));
                stb_result.AppendLine("     showButtonPanel:false,");
                stb_result.AppendLine("\t    showTimepicker:false, changeMonth: true, changeYear: true, ");
                stb_result.AppendLine("\t    dateFormat: \"dd-mm-yy\",");
                stb_result.AppendLine("     onClose: function(dateText, inst) {");
                stb_result.AppendLine(string.Concat("         var endDateTextBox = $(\"#", this.ToDateID, "\");"));
                stb_result.AppendLine("         if (endDateTextBox.val() != \"\") {");
                stb_result.AppendLine("             var testStartDate = new Date(dateText);");
                stb_result.AppendLine("             var testEndDate = new Date(endDateTextBox.val());");
                stb_result.AppendLine("             if (testStartDate > testEndDate)");
                stb_result.AppendLine("                 endDateTextBox.val(dateText);");
                stb_result.AppendLine("         }");
                stb_result.AppendLine("         else {");
                stb_result.AppendLine("             endDateTextBox.val(dateText);");
                stb_result.AppendLine("         }");
                stb_result.AppendLine(string.Concat("         var start = $(\"#", this.ToDateID, "\").datetimepicker('getDate');"));
                stb_result.AppendLine("         var month = (start.getMonth()+1);");
                stb_result.AppendLine("         if(String(month).length==1) month = \"0\" + (start.getMonth()+1);");
                stb_result.AppendLine("\t            var NewEndDate =  daysInMonth(start.getMonth()+1, start.getFullYear()) + \"-\" + month + \"-\" + start.getFullYear();");
                stb_result.AppendLine(string.Concat("\t            $(\"#", this.ToDateID, "\").val(NewEndDate);"));
                stb_result.AppendLine("         },");
                stb_result.AppendLine("     beforeShow: function() {");
                stb_result.AppendLine("         setTimeout(function(){");
                stb_result.AppendLine("             $('.ui-datepicker').css('z-index', 9999);");
                stb_result.AppendLine("         }, 0);");
                stb_result.AppendLine("     },");
                stb_result.AppendLine("     onSelect: function (selectedDateTime){");
                stb_result.AppendLine("         var start = $(this).datetimepicker(\"getDate\");");
                stb_result.AppendLine(string.Concat("         $(\"#", this.ToDateID, "\").datetimepicker(\"option\", \"minDate\", new Date(start.getTime()));"));
                stb_result.AppendLine("     }");
                stb_result.AppendLine(" });");
                stb_result.AppendLine(string.Concat(" $(\"#", this.ToDateID, "\").datetimepicker({"));
                stb_result.AppendLine("     showButtonPanel:false,");
                stb_result.AppendLine("     showTimepicker:false, changeMonth: true, changeYear: true,");
                stb_result.AppendLine("     dateFormat: \"dd-mm-yy\",");
                stb_result.AppendLine("     onClose: function(dateText, inst) {");
                stb_result.AppendLine(string.Concat("         var startDateTextBox = $(\"#", this.FromDateID, "\");"));
                stb_result.AppendLine("         if (startDateTextBox.val() != \"\") {");
                stb_result.AppendLine("             var testStartDate = new Date(startDateTextBox.val());");
                stb_result.AppendLine("             var testEndDate = new Date(dateText);");
                stb_result.AppendLine("             if (testStartDate > testEndDate)");
                stb_result.AppendLine("                 startDateTextBox.val(dateText);");
                stb_result.AppendLine("             }");
                stb_result.AppendLine("             else {");
                stb_result.AppendLine("                 startDateTextBox.val(dateText);");
                stb_result.AppendLine("             }");
                stb_result.AppendLine("         },");
                stb_result.AppendLine("     beforeShow: function() {");
                stb_result.AppendLine("         setTimeout(function(){");
                stb_result.AppendLine("             $('.ui-datepicker').css('z-index', 9999);");
                stb_result.AppendLine("         }, 0);");
                stb_result.AppendLine("     },");
                stb_result.AppendLine("     onSelect: function (selectedDateTime){");
                stb_result.AppendLine("         var end = $(this).datetimepicker(\"getDate\");");
                stb_result.AppendLine("     }");
                stb_result.AppendLine(" });");

                stb_result.AppendLine(string.Concat(" var Dates = new Date(\"", easternTimeNow.ToString("YYYY/MM/dd"), "\");"));
                stb_result.AppendLine(" var Months = (Dates.getMonth()+1);");
                stb_result.AppendLine(" if (String(Months).length==1) Months = \"0\" + (Dates.getMonth()+1);");
                stb_result.AppendLine(" var FromDate = Dates.getFullYear() + \"-\" + Months + \"-01\";");
                stb_result.AppendLine(" var LastDay = daysInMonth(Dates.getMonth()+1, Dates.getFullYear());");
                stb_result.AppendLine(" var ToDate =  Dates.getFullYear() + \"-\" + Months + \"-\" + LastDay;");
                if (!(this.FromDateValue == null || !(this.FromDateValue != "")))
                {
                    labelAlign = new string[] { " $(\"#", this.FromDateID, "\").datetimepicker('setDate', new Date('", this.FromDateValue, "'));" };
                    stb_result.AppendLine(string.Concat(labelAlign));
                }
                else if (this.DefaultValueDate)
                {
                    stb_result.AppendLine(string.Concat(" $(\"#", this.FromDateID, "\").datetimepicker('setDate', new Date(FromDate));"));
                }
                if (!(this.ToDateValue == null || !(this.ToDateValue != "")))
                {
                    labelAlign = new string[] { " $(\"#", this.ToDateID, "\").datetimepicker(\"setDate\", new Date('", this.ToDateValue, "'));" };
                    stb_result.AppendLine(string.Concat(labelAlign));
                }
                else if (this.DefaultValueDate)
                {
                    stb_result.AppendLine(string.Concat(" $(\"#", this.ToDateID, "\").datetimepicker(\"setDate\", new Date(ToDate));"));
                }
                stb_result.AppendLine("});");
                stb_result.AppendLine("</script>");
            }
            return stb_result.ToString();
        }
    }
}