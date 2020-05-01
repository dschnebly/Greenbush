using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenBushIEP.Models
{
    public class StudentGoal
    {
        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public tblGoal goal { get; set; }
        public string modifiedByFullName { get; set; }
        public List<tblGoalBenchmark> benchmarks { get; set; } = new List<tblGoalBenchmark>();
        public List<tblGoalEvaluationProcedure> evaluationProcedures { get; set; } = new List<tblGoalEvaluationProcedure>();
        public List<tblGoalBenchmarkMethod> shortTermBenchmarkMethods { get; set; } = new List<tblGoalBenchmarkMethod>();

        public StudentGoal(int? goalId = null)
        {
            if (goalId != null && goalId != 0)
            {
                goal = db.tblGoals.Where(g => g.goalID == goalId).FirstOrDefault();
                benchmarks = db.tblGoalBenchmarks.Where(o => o.goalID == goalId).ToList();
                evaluationProcedures = db.tblGoalEvaluationProcedures.Where(o => o.goalID == goalId).ToList();

                tblUser modifier = db.tblUsers.Where(u => u.UserID == goal.ModifiedBy).SingleOrDefault();
                modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
            }
            else
            {
                goal = new tblGoal();
                benchmarks = new List<tblGoalBenchmark>();
                evaluationProcedures = new List<tblGoalEvaluationProcedure>();
            }
        }

        public void SaveGoal(string evalProcedures, string otherDesc)
        {
            tblGoal ourGoal = db.tblGoals.Where(g => g.goalID == goal.goalID).FirstOrDefault();
            if (ourGoal == null)
            {
                goal.Create_Date = DateTime.Now;
                goal.ProgressDate_Quarter1 = DateTime.Now;
                goal.ProgressDate_Quarter2 = DateTime.Now;
                goal.ProgressDate_Quarter3 = DateTime.Now;
                goal.ProgressDate_Quarter4 = DateTime.Now;
                db.tblGoals.Add(goal);
            }

            goal.Update_Date = DateTime.Now;
            db.SaveChanges();

            foreach (tblGoalBenchmark benchmark in benchmarks)
            {
                tblGoalBenchmark currentBenchmark = db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == benchmark.goalBenchmarkID).FirstOrDefault();
                if (currentBenchmark != null)
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
                    currentBenchmark.Update_Date = DateTime.Now;
                }
                else
                {
                    tblGoalBenchmark goalBenchmark = new tblGoalBenchmark
                    {
                        goalID = goal.goalID,
                        Method = benchmark.Method,
                        ObjectiveBenchmark = benchmark.ObjectiveBenchmark,
                        ProgressDate_Quarter1 = DateTime.Now,
                        ProgressDate_Quarter2 = DateTime.Now,
                        ProgressDate_Quarter3 = DateTime.Now,
                        ProgressDate_Quarter4 = DateTime.Now,
                        TransitionActivity = benchmark.TransitionActivity,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };

                    db.tblGoalBenchmarks.Add(goalBenchmark);
                }

                db.SaveChanges();
            }

            foreach (tblGoalBenchmarkMethod benchmarkMethod in shortTermBenchmarkMethods)
            {
                List<tblGoalBenchmarkMethod> currentMethods = db.tblGoalBenchmarkMethods.Where(b => b.goalBenchmarkID == benchmarkMethod.goalBenchmarkID).ToList();
                foreach (tblGoalBenchmarkMethod cm in currentMethods)
                {
                    db.tblGoalBenchmarkMethods.Remove(cm);
                }
            }
            db.SaveChanges();

            foreach (tblGoalBenchmarkMethod benchmarkMethod in shortTermBenchmarkMethods)
            {
                tblGoalBenchmarkMethod currentMethod = db.tblGoalBenchmarkMethods.Where(b => b.goalBenchmarkID == benchmarkMethod.goalBenchmarkID).FirstOrDefault();
                if (currentMethod == null)
                {
                    db.tblGoalBenchmarkMethods.Add(benchmarkMethod);
                }
            }
            db.SaveChanges();

            if (!string.IsNullOrEmpty(evalProcedures))
            {
                string[] evalProceduresArray = evalProcedures.Split(',');
                IQueryable<tblGoalEvaluationProcedure> currentList = db.tblGoalEvaluationProcedures.Where(o => o.goalID == goal.goalID);

                foreach (tblGoalEvaluationProcedure obj in currentList)
                {
                    db.tblGoalEvaluationProcedures.Remove(obj);
                }

                db.SaveChanges();//delete exting rows

                foreach (string evalProc in evalProceduresArray)
                {
                    int.TryParse(evalProc, out int evalProcVal);

                    if (evalProcVal > 0)
                    {
                        db.tblGoalEvaluationProcedures.Add(new tblGoalEvaluationProcedure() { goalID = goal.goalID, evaluationProcedureID = evalProcVal, Create_Date = DateTime.Now, OtherDescription = otherDesc, Update_Date = DateTime.Now });
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