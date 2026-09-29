using System;
using System.Collections.Generic;
using System.Text;

namespace library_pr1
{
    /// <summary>
    /// Хранит результат анализа сборки <see cref="Character"/>: все рассчитанные
    /// боевые характеристики и итоговый качественный рейтинг.
    /// </summary>
    public class BuildAnalysisResult
    {
        /// <summary>
        /// Итоговая атака (базовая атака + атака оружия).
        /// </summary>
        public double FinalAttack { get; }

        /// <summary>
        /// Урон, наносимый при критическом ударе.
        /// </summary>
        public double CriticalDamage { get; }

        /// <summary>
        /// Ожидаемый средний урон за удар, с учётом шанса критического удара.
        /// </summary>
        public double AverageDamage { get; }

        /// <summary>
        /// Шанс критического удара, в процентах, перенесённый из исходного персонажа.
        /// </summary>
        public double CriticalChancePercent { get; }

        /// <summary>
        /// Итоговый качественный рейтинг сборки.
        /// </summary>
        public BuildRating Rating { get; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="BuildAnalysisResult"/>.
        /// </summary>
        /// <param name="finalAttack">Рассчитанная итоговая атака.</param>
        /// <param name="criticalDamage">Рассчитанный критический урон.</param>
        /// <param name="averageDamage">Рассчитанный средний ожидаемый урон.</param>
        /// <param name="criticalChancePercent">Шанс критического удара, в процентах.</param>
        /// <param name="rating">Итоговый рейтинг сборки.</param>
        public BuildAnalysisResult(
            double finalAttack,
            double criticalDamage,
            double averageDamage,
            double criticalChancePercent,
            BuildRating rating)
        {
            FinalAttack = finalAttack;
            CriticalDamage = criticalDamage;
            AverageDamage = averageDamage;
            CriticalChancePercent = criticalChancePercent;
            Rating = rating;
        }
    }
}
