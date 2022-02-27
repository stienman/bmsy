internal class BMSTask
    {
        public BMSTask() { }

        public BMSTask(TaskInfo reason)
        {
            TaskInfo = reason;
            Executed = DateTime.Now;
        }
        public TaskInfo TaskInfo { get; set; }
        public DateTime Executed { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime StartTime { get; set; }
        public int TargetSOC { get; set; }
    }

