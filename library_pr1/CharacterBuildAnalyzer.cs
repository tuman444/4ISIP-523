using System;
using System.Collections.Generic;
using System.Text;

namespace library_pr1
{
    /// <summary>
    /// Выполняет боевые расчёты для сборки <see cref="Character"/>: итоговую атаку,
    /// критический урон, средний урон и итоговый рейтинг сборки.
    /// Этот класс не зависит от консоли или любого другого механизма ввода/вывода —
    /// он работает только с данными в памяти.
    /// </summary>
    public static class CharacterBuildAnalyzer
    {
        /// <summary>
        /// Порог среднего урона, ниже которого сборке присваивается рейтинг
        /// <see cref="BuildRating.Weak"/>.
        /// </summary>
        private const double WeakUpperBound = 1500;

        /// <summary>
        /// Порог среднего урона, начиная с которого сборке присваивается рейтинг
        /// <see cref="BuildRating.Good"/> (пока не достигнут <see cref="ExcellentLowerBound"/>).
        /// </summary>
        private const double GoodLowerBound = 2500;

        /// <summary>
        /// Порог среднего урона, начиная с которого сборке присваивается рейтинг
        /// <see cref="BuildRating.Excellent"/>.
        /// </summary>
        private const double ExcellentLowerBound = 3500;

        /// <summary>
        /// Выполняет полный анализ сборки персонажа: рассчитывает итоговую атаку,
        /// критический урон, средний урон и качественный рейтинг сборки.
        /// </summary>
        /// <param name="character">Сборка персонажа для анализа.</param>
        /// <returns><see cref="BuildAnalysisResult"/> со всеми рассчитанными характеристиками.</returns>
        public static BuildAnalysisResult Analyze(Character character)
        {
            double finalAttack = CalculateFinalAttack(character.BaseAttack, character.WeaponAttack);

            double criticalDamage = CalculateCriticalDamage(finalAttack, character.CriticalDamagePercent);

            double averageDamage = CalculateAverageDamage(
                finalAttack,
                character.CriticalChancePercent,
                character.CriticalDamagePercent);

            BuildRating rating = DetermineRating(averageDamage);

            return new BuildAnalysisResult(
                finalAttack,
                criticalDamage,
                averageDamage,
                character.CriticalChancePercent,
                rating);
        }

        /// <summary>
        /// Рассчитывает итоговую атаку.
        /// Формула: Итоговая атака = Базовая атака + Атака оружия.
        /// </summary>
        /// <param name="baseAttack">Базовая атака.</param>
        /// <param name="weaponAttack">Атака оружия.</param>
        /// <returns>Итоговая атака.</returns>
        public static double CalculateFinalAttack(double baseAttack, double weaponAttack)
        {
            return baseAttack + weaponAttack;
        }

        /// <summary>
        /// Рассчитывает урон, наносимый при критическом ударе.
        /// Формула: Критический урон = Итоговая атака * (1 + Критический урон в процентах / 100).
        /// </summary>
        /// <param name="finalAttack">Итоговая атака.</param>
        /// <param name="criticalDamagePercent">Бонус критического урона, в процентах.</param>
        /// <returns>Критический урон.</returns>
        public static double CalculateCriticalDamage(double finalAttack, double criticalDamagePercent)
        {
            return finalAttack * (1 + criticalDamagePercent / 100.0);
        }

        /// <summary>
        /// Рассчитывает ожидаемый средний урон за один удар с учётом шанса критического удара.
        /// Формула: Средний урон = Итоговая атака * (1 + (Шанс крит. удара / 100) * (Крит. урон / 100)).
        /// </summary>
        /// <param name="finalAttack">Итоговая атака.</param>
        /// <param name="criticalChancePercent">Шанс критического удара, в процентах.</param>
        /// <param name="criticalDamagePercent">Бонус критического урона, в процентах.</param>
        /// <returns>Средний ожидаемый урон за удар.</returns>
        public static double CalculateAverageDamage(
            double finalAttack,
            double criticalChancePercent,
            double criticalDamagePercent)
        {
            return finalAttack * (1 + (criticalChancePercent / 100.0) * (criticalDamagePercent / 100.0));
        }

        /// <summary>
        /// Определяет качественный рейтинг сборки по значению среднего урона,
        /// согласно шкале: менее 1500 - Слабая; 1500-2499 - Нормальная;
        /// 2500-3499 - Хорошая; 3500 и более - Отличная.
        /// </summary>
        /// <param name="averageDamage">Средний ожидаемый урон за удар.</param>
        /// <returns>Соответствующий <see cref="BuildRating"/>.</returns>
        public static BuildRating DetermineRating(double averageDamage)
        {
            if (averageDamage < WeakUpperBound)
            {
                return BuildRating.Weak;
            }

            if (averageDamage < GoodLowerBound)
            {
                return BuildRating.Normal;
            }

            if (averageDamage < ExcellentLowerBound)
            {
                return BuildRating.Good;
            }

            return BuildRating.Excellent;
        }
    }
}
