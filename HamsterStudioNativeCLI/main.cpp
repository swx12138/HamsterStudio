
#include <memory>
#include <string>

namespace Multiplier
{
	class Attack {
	public:
		Attack(double attack, double weapon_attack, double percent_bouns, double fixed_bouns)
			: attack_(attack), weapon_attack_(weapon_attack),
			percent_bouns_(percent_bouns), fixed_bouns_(fixed_bouns) {
			final_attack_ = (attack_ + weapon_attack_) * (1 + percent_bouns_) + fixed_bouns_;
		}

		double GetFinalAttack() const {
			return final_attack_;
		}

	private:
		double attack_;
		double weapon_attack_;
		double percent_bouns_;
		double fixed_bouns_;

		double final_attack_;
	};

	class DamageMultipiler {
	public:
		explicit DamageMultipiler(std::string_view name, double ratio, double buff)
			:name_(name), ratio_(ratio), buff_(buff) 
		{
			final_multiplier_ = ratio_ * (1 + buff_);
		}

		double GetFinalMultiplier() const {
			return final_multiplier_;
		}

		std::string_view GetName() const {
			return name_;
		}

	private:
		double ratio_;
		double buff_;
		std::string_view name_;

		double final_multiplier_;
	};

	class DamageBonus {
	public:
		DamageBonus(double bonus, double element_bonus, double skill_bonus, double damage_bonus)
			: bonus_(bonus), element_bonus_(element_bonus),
			skill_bonus_(skill_bonus), damage_bonus_(damage_bonus) {
			total_bonus_ = 1.0 + bonus_ + element_bonus_ + skill_bonus_ + damage_bonus_;
		}

		double GetTotalBonus() const {
			return total_bonus_;
		}

	private:
		double bonus_;
		double element_bonus_;
		double skill_bonus_;
		double damage_bonus_;

		double total_bonus_;
	};

	class CriticalBracket {
	public:
		CriticalBracket(double crit_rate, double crit_damage)
			: crit_rate_(crit_rate), crit_damage_(crit_damage) { }
		double GetFinalCritMultiplier(bool alwaysCritical) const {
			return alwaysCritical ? (1.0 + crit_damage_) : (1.0 + crit_rate_ * crit_damage_);
		}

	private:
		double crit_rate_;
		double crit_damage_;
	};

	class ElementReactionBonus {
	public:

		explicit ElementReactionBonus(double element_mastery)
			: element_mastery_(element_mastery) { }

		double GetReactionMultiplier(double reaction_multiplier, double react_bonus) const {
			double element_mastery_bonus = CalculateElementMasteryBonus(element_mastery_);
			return reaction_multiplier * (1 + element_mastery_bonus + react_bonus);
		}

		static double CalculateElementMasteryBonus(double element_mastery) {
			return 2.78 * element_mastery / (element_mastery + 1400);
		}
	private:
		double element_mastery_;
	};

	class Defense {
	public:
		explicit Defense(double target_level, double charactor_level, double defense_reduction, double defense_ignore) noexcept
			: target_level_(target_level), charactor_level_(charactor_level),
			defense_reduction_(defense_reduction), defense_ignore_(defense_ignore) { }
		constexpr double GetDefenseMultiplier() const {
			double level_up = (charactor_level_ + 100);
			double effective_defense = (1 - defense_ignore_) * (1 - defense_reduction_) * (target_level_ + 100);
			return level_up / (level_up + effective_defense);
		}
	private:
		double target_level_;
		double charactor_level_;
		double defense_reduction_;
		double defense_ignore_;
	};

	class Resistance {
	public:
		explicit Resistance(double resistance) noexcept : resistance_(resistance) { }

		constexpr double GetResistanceMultiplier() const {
			if (resistance_ < 0) {
				return 1 - (resistance_ / 2);
			}
			else if (resistance_ >= 0 && resistance_ <= 0.75) {
				return 1 - resistance_;
			}
			else {
				return 1 / (4 * resistance_ + 1);
			}
		}

	private:
		double resistance_;
	};

}


#include <iostream>
#include <vector>

// https://nga.178.com/read.php?tid=25564438
int main()
{
	using namespace Multiplier;

	// 伤害=攻击区*倍率区*增伤区*暴击区*反应区*防御区*抗性区

	auto attack = Attack(512, 0, 0, 411); // 60级兹白带80级黎明神剑

	auto damage_bonus = DamageBonus(0, 0, 0, 0);   // 月岩666+999防御-兹白不吃
	auto critical_bracket = CriticalBracket(0.688, 2.410); // 23.9%暴击率，55%暴击伤害

	auto element_reaction_bonus = ElementReactionBonus(74); // 74元素精通

	auto defense = Defense(100, 60, 0, 0); //  94级怪
	auto resistance = Resistance(0.1); // 木桩10%全抗

	auto damage = attack.GetFinalAttack();
	//damage *= damage_multiplier.GetFinalMultiplier();
	damage *= damage_bonus.GetTotalBonus();
	//damage *=  critical_bracket.GetFinalCritMultiplier(false);

	//double element_reaction_bonus_v =  element_reaction_bonus.GetReactionMultiplier(1.5, 0.15);
	//damage *= element_reaction_bonus_v; // 平A不吃精通

	damage *= defense.GetDefenseMultiplier();
	damage *= resistance.GetResistanceMultiplier();

	double buff = 0;
	auto actions = std::vector<DamageMultipiler> {
		DamageMultipiler("一段平A", 0.735, buff), 	// 实测不暴击91，暴击312
		DamageMultipiler("二段平A", 0.677, buff),
		DamageMultipiler("三段平A*2", 0.449, buff),
		DamageMultipiler("四段平A", 1.132, buff),
		DamageMultipiler("重击*2", 1.071, buff),
		DamageMultipiler("下落", 0.929, buff),
		DamageMultipiler("低空冲击", 1.86, buff),
		DamageMultipiler("高空冲击", 2.32, buff),
	};

	printf_s("%-10s \t %9s \t %9s \t %9s\n", "动作", "伤害", "暴击伤害", "期望");
	for (auto &action : actions) {
		auto action_damage = damage * action.GetFinalMultiplier();
		auto action_crit_damage = action_damage * critical_bracket.GetFinalCritMultiplier(true);
		auto action_expected_damage = action_damage * critical_bracket.GetFinalCritMultiplier(false);
		printf_s("%-10s \t %9.2f \t %9.2lf \t %9.2lf\n", action.GetName().data(), action_damage, action_crit_damage, action_expected_damage);
	}

	return 0;
}