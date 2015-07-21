﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiClickTestDSL.AutomationCode;

namespace UiClickTestDSL.DslObjects {
    public class GuiComboBox {
        public static IEnumerable<AutomationElement> GetAll(AutomationElement window) {
            var res = window.FindAllChildrenByControlType(ControlType.ComboBox);
            return res;
        }

        public static GuiComboBox Find(AutomationElement window, string automationId) {
            var res = window.FindChildByControlTypeAndAutomationId(ControlType.ComboBox, automationId);
            return new GuiComboBox(res);
        }


        private readonly AutomationElement _cmb;
        private readonly ExpandCollapsePattern _expandCollapse;

        public GuiComboBox(AutomationElement comboBox) {
            _cmb = comboBox;
            _expandCollapse = _cmb.GetPattern<ExpandCollapsePattern>(ExpandCollapsePattern.Pattern);
        }

        private void Activate() {
            _cmb.SetFocus();
            _expandCollapse.Expand();
            _expandCollapse.Collapse();
        }

        public GuiComboBoxItem SelectedItem {
            get {
                List<GuiComboBoxItem> all = GetAllItems();
                IEnumerable<GuiComboBoxItem> item = from i in all
                                                    where i.IsSelected
                                                    select i;
                return item.First();
            }
        }

        public string DisplayText {
            get {
                List<GuiComboBoxItem> all = GetAllItems();
                IEnumerable<string> displayText = from i in all
                                                  where i.IsSelected
                                                  select i.Text;
                return displayText.First();
            }
        }

        public void SelectItem(int i) {
            var all = GetAllItems();
            Assert.IsTrue(all.Count > i, "Not enough items to select #" + i);
            all[i].Select();
        }

        public void SelectItem(string caption) {
            List<GuiComboBoxItem> all = GetAllItems();
            IEnumerable<GuiComboBoxItem> item = from i in all
                                                where RegexMatch(i.Text.Trim(), caption)
                                                select i;
            item.First().Select();
        }

        public void SelectItemContaining(string caption) {
            List<GuiComboBoxItem> all = GetAllItems();
            IEnumerable<GuiComboBoxItem> item = from i in all
                                                where i.Text.Contains(caption)
                                                select i;
            item.First().Select();
        }

        private static bool RegexMatch(string text, string caption) {
            return text == caption || Regex.IsMatch(text, @"(.* name\:|\[.*,) " + caption + @"(\]){0,1}") || text.Trim().EndsWith(": " + caption);
        }

        public List<GuiComboBoxItem> GetAllItems() {
            Activate();
            IEnumerable<AutomationElement> all = _cmb.FindAllChildrenByControlType(ControlType.ListItem);
            return all.Select(comboBoxItem => new GuiComboBoxItem(comboBoxItem)).ToList();
        }

        public void PrintAllItems() {
            foreach (var item in GetAllItems()) {
                Console.WriteLine(item.Text);
            }
        }

        public void ShouldRead(string text) {
            string displayed = DisplayText;
            Assert.IsTrue(RegexMatch(displayed, text), "Wrong value in combobox, should be: " + text + " was: " + displayed);
        }

        public void ShouldContainItems() {
            Assert.AreNotEqual(0, GetAllItems().Count);
        }
    }
}