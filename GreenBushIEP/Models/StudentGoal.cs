using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreenBushIEP.Models
{
    public class StudentGoal
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public tblGoal goal { get; set; }
        public List<tblGoalBenchmark> benchmarks { get; set; } = new List<tblGoalBenchmark>();
        public List<tblGoalEvaluationProcedure> evaluationProcedures { get; set; } = new List<tblGoalEvaluationProcedure>();

        public StudentGoal(int? goalId = null)
        {  
            if (goalId != null && goalId != 0)
            {
                this.goal = db.tblGoals.Where(g => g.goalID == goalId).FirstOrDefault();
                this.benchmarks = db.tblGoalBenchmarks.Where(o => o.goalID == goalId).ToList();
                this.evaluationProcedures = db.tblGoalEvaluationProcedures.Where(o => o.goalID == goalId).ToList();
            }
            else
            {
                this.goal = new tblGoal();
                this.benchmarks = new List<tblGoalBenchmark>();
                this.evaluationProcedures = new List<tblGoalEvaluationProcedure>();
            }
        }

        public void SaveGoal(string evalProcedures)
        {
            tblGoal ourGoal = db.tblGoals.Where(g => g.goalID == this.goal.goalID).FirstOrDefault();
            if (ourGoal == null)
            {
                this.goal.Create_Date = DateTime.Now;
                db.tblGoals.Add(this.goal);
            }

            this.goal.Update_Date = DateTime.Now;
            db.SaveChanges();

            foreach (tblGoalBenchmark benchmark in this.benchmarks)
            {
                tblGoalBenchmark currentBenchmark = db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == benchmark.goalBenchmarkID).FirstOrDefault();
                if(currentBenchmark != null)
                {
                    currentBenchmark.goalID = benchmark.goalID;
                    currentBenchmark.Method = benchmark.Method;
                    currentBenchmark.ObjectiveBenchmark = benchmark.ObjectiveBenchmark;
                    currentBenchmark.ProgressDate_Quarter1 = benchmark.ProgressDate_Quarter1;
                    currentBenchmark.ProgressDate_Quarter2 = benchmark.ProgressDate_Quarter2;
                    currentBenchmark.ProgressDate_Quarter3 = benchmark.ProgressDate_Quarter3;
                    currentBenchmark.ProgressDate_Quarter4 = benchmark.ProgressDate_Quarter4;
                    currentBenchmark.ProgressDescription_Quarter1 = benchmark.ProgressDescription_Quarter1;
                    currentBenchmark.ProgressDescription_Quarter2 = benchmark.ProgressDescription_Quarter2;
                    currentBenchmark.ProgressDescription_Quarter3 = benchmark.ProgressDescription_Quarter3;
                    currentBenchmark.ProgressDescription_Quarter4 = benchmark.ProgressDescription_Quarter4;
                    currentBenchmark.Progress_Quarter1 = benchmark.Progress_Quarter1;
                    currentBenchmark.Progress_Quarter2 = benchmark.Progress_Quarter2;
                    currentBenchmark.Progress_Quarter3 = benchmark.Progress_Quarter3;
                    currentBenchmark.Progress_Quarter4 = benchmark.Progress_Quarter4;
                    currentBenchmark.TransitionActivity = benchmark.TransitionActivity;
                }
                else
                {
                    benchmark.goalID = goal.goalID;
                    benchmark.Create_Date = DateTime.Now;
                    db.tblGoalBenchmarks.Add(benchmark);
                }

                benchmark.Update_Date = DateTime.Now;
                db.SaveChanges();
            }

            if (!string.IsNullOrEmpty(evalProcedures))
            {
                var evalProceduresArray = evalProcedures.Split(',');
                var currentList = db.tblGoalEvaluationProcedures.Where(o => o.goalID == this.goal.goalID);
                foreach (tblGoalEvaluationProcedure obj in currentList)
                {
                    db.tblGoalEvaluationProcedures.Remove(obj);                    
                }
                db.SaveChanges();//delete exting rows

                foreach (var evalProc in evalProceduresArray)
                {
                    int evalProcVal = 0;
                    Int32.TryParse(evalProc, out evalProcVal);

                    if (evalProcVal > 0)
                    {                        
                        db.tblGoalEvaluationProcedures.Add(new tblGoalEvaluationProcedure() { goalID = this.goal.goalID, evaluationProcedureID = evalProcVal, Create_Date = DateTime.Now });                        
                    }
                }
                db.SaveChanges(); //save new rows
            }
        }

        public void DeleteGoal()
        {
            foreach (tblGoalBenchmark obj in benchmarks)
            {
                db.tblGoalBenchmarks.Remove(obj);
                db.SaveChanges();
            }

            foreach (tblGoalEvaluationProcedure objEP in evaluationProcedures)
            {
                db.tblGoalEvaluationProcedures.Remove(objEP);
                db.SaveChanges();
            }

            db.tblGoals.Remove(goal);
            db.SaveChanges();
        }
    }
}