using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TodoAppGUI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TodoForm());
        }
    }

    public class TodoForm : Form
    {
        private TextBox taskInput;
        private DateTimePicker dueDatePicker;
        private Button addButton;
        private Button editButton;
        private ListBox taskList;
        private Button removeButton;
        private Button saveButton;
        private Button loadButton;

        private NotifyIcon trayIcon;
        private System.Windows.Forms.Timer reminderTimer;

        private readonly string savePath = "tasks.txt";

        public TodoForm()
        {
            this.Text = "To-Do App";
            this.Width = 450;
            this.Height = 550;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Task input
            taskInput = new TextBox() {Text = "Please enter your task here!", Left = 20, Top = 20, Width = 280 };
            this.Controls.Add(taskInput);

            dueDatePicker = new DateTimePicker() { Left = 20, Top = 50, Width = 280 };
            this.Controls.Add(dueDatePicker);

            // Add button
            addButton = new Button() { Text = "Add Task", Left = 310, Top = 20, Width = 100 };
            addButton.Click += AddButton_Click;
            this.Controls.Add(addButton);

            // Edit button
            editButton = new Button() { Text = "Edit Task", Left = 310, Top = 50, Width = 100 };
            editButton.Click += EditButton_Click;
            this.Controls.Add(editButton);

            // Task list
            taskList = new ListBox() { Left = 20, Top = 90, Width = 390, Height = 300 };
            taskList.MouseDoubleClick += TaskList_MouseDoubleClick;
            this.Controls.Add(taskList);

            // Remove button
            removeButton = new Button() { Text = "Remove Selected Task", Left = 20, Top = 400, Width = 180 };
            removeButton.Click += RemoveButton_Click;
            this.Controls.Add(removeButton);

            // Save button
            saveButton = new Button() { Text = "Save Tasks", Left = 220, Top = 400, Width = 90 };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            // Load button
            loadButton = new Button() { Text = "Load Tasks", Left = 320, Top = 400, Width = 90 };
            loadButton.Click += LoadButton_Click;
            this.Controls.Add(loadButton);

            // Tray icon and timer
            trayIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information,
                BalloonTipTitle = "Task Reminder"
            };

            reminderTimer = new System.Windows.Forms.Timer();
            reminderTimer.Interval = 60000; // every 1 minute
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            string task = taskInput.Text.Trim();
            DateTime due = dueDatePicker.Value;

            if (!string.IsNullOrEmpty(task))
            {
                taskList.Items.Add($"{task} | Due: {due:yyyy-MM-dd HH:mm}");
                taskInput.Clear();
            }
            else
            {
                MessageBox.Show("Task cannot be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            if (taskList.SelectedItem != null)
            {
                string currentItem = taskList.SelectedItem.ToString()!;
                string[] parts = currentItem.Split(" | Due: ");
                if (parts.Length == 2)
                {
                    taskInput.Text = parts[0];
                    if (DateTime.TryParse(parts[1], out DateTime due))
                    {
                        dueDatePicker.Value = due;
                    }

                    taskList.Items.Remove(taskList.SelectedItem);
                }
            }
        }

        private void TaskList_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            int index = taskList.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                string selectedItem = taskList.Items[index].ToString()!;
                string[] parts = selectedItem.Split(" | Due: ");
                if (parts.Length == 2)
                {
                    taskInput.Text = parts[0];
                    if (DateTime.TryParse(parts[1], out DateTime due))
                    {
                        dueDatePicker.Value = due;
                    }

                    taskList.Items.RemoveAt(index);
                }
            }
        }

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            if (taskList.SelectedIndex != -1)
            {
                taskList.Items.RemoveAt(taskList.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Please select a task to remove.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            File.WriteAllLines(savePath, taskList.Items.Cast<string>());
            MessageBox.Show("Tasks saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            if (File.Exists(savePath))
            {
                string[] lines = File.ReadAllLines(savePath);
                taskList.Items.Clear();
                foreach (var line in lines)
                {
                    taskList.Items.Add(line);
                }
            }
        }

        private void ReminderTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            foreach (string item in taskList.Items)
            {
                string[] parts = item.Split(" | Due: ");
                if (parts.Length == 2 && DateTime.TryParse(parts[1], out DateTime due))
                {
                    if (due <= now.AddMinutes(1) && due > now.AddSeconds(-30))
                    {
                        trayIcon.BalloonTipText = $"Task Due: {parts[0]}";
                        trayIcon.ShowBalloonTip(3000);
                    }
                }
            }
        }
    }
}
