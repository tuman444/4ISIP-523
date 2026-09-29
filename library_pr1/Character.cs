using System;
using System.Collections.Generic;
using System.Text;

namespace library_pr1
{
    /// <summary>
    /// Представляет игровую сборку персонажа с базовыми боевыми параметрами, которые вводит пользователь.
    /// </summary>
    public class Character
    {
        ///<summary>
        /// Имя персонажа
        public string Name { get; set; }
        /// базовое значение атаки персонажа
        public double BaseAttack { get; set; }
        /// доп значение атаки персонажа
        public double WeaponAttack { get; set; }
        /// шанс нанесения крит.урона
        public double CriticalChancePercent { get; set; }
        /// бонусный урон при крит.уроне
        public double CriticalDamagePercent { get; set; }
        /// </summary>

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="Character"/>.
        /// </summary>
        /// <param name="name">Имя персонажа.</param>
        /// <param name="baseAttack">Базовая атака персонажа.</param>
        /// <param name="weaponAttack">Атака оружия.</param>
        /// <param name="criticalChancePercent">Шанс критического удара, в процентах.</param>
        /// <param name="criticalDamagePercent">Бонус критического урона, в процентах.</param>
        public Character(string name, double baseAttack, double weaponAttack, double criticalChancePercent, double criticalDamagePercent)
        {
            Name = name;
            BaseAttack = baseAttack;
            WeaponAttack = weaponAttack;
            CriticalChancePercent = criticalChancePercent;
            CriticalDamagePercent = criticalDamagePercent;
        }

    }
}
