#pragma once

#include <string>
#include <sstream>
#include <vector>
#include <memory>
#include <array>
#include <type_traits>
#include <utility>
#include <cstddef>

#define HTML_DOCUMENT_NAMESPACE HamsterStudioToes::HtmlDocument

namespace HTML_DOCUMENT_NAMESPACE
{
	namespace Detail
	{
		/// <summary>
		/// 对 HTML 文本中的特殊字符进行转义，防止注入与格式破损。
		/// </summary>
		inline std::string HtmlEncode(std::string_view text)
		{
			std::string result;
			result.reserve(text.size());
			for (auto ch : text)
			{
				switch (ch)
				{
				case '&':  result.append("&amp;");  break;
				case '<':  result.append("&lt;");   break;
				case '>':  result.append("&gt;");   break;
				case '"':  result.append("&quot;"); break;
				default:   result.push_back(ch);    break;
				}
			}
			return result;
		}
	}
}

namespace HTML_DOCUMENT_NAMESPACE
{
	class HtmlElementProperty {
	public:
		virtual	operator std::string() const = 0;
		friend std::ostream &operator<<(std::ostream &os, HtmlElementProperty const &prop) {
			prop.Print(os);
			return os;
		}
	protected:
		virtual void Print(std::ostream &os) const = 0;
	};

	class KeyValueProperty : public HtmlElementProperty {
		std::string _key;
		std::string _value;
	public:
		explicit KeyValueProperty(std::string_view key, std::string_view value)
			: _key(key), _value(value)
		{ }

		KeyValueProperty(KeyValueProperty &&other) noexcept
			: _key(std::move(other._key)), _value(std::move(other._value)) { }

		virtual void Print(std::ostream &os) const override {
			os << _key << "=\"" << Detail::HtmlEncode(_value) << "\"";
		}

		virtual	operator std::string() const override
		{
			std::ostringstream oss;
			oss << *this;
			return oss.str();
		}
	};

	class HtmlElement {
	public:
		std::string TagName;
		std::vector<std::shared_ptr<HtmlElementProperty>> Properties;

		using HtmlElementPointer = std::shared_ptr<HtmlElement>;
		std::vector<HtmlElementPointer> Children;

	public:
		HtmlElement() = default;

		HtmlElement(HtmlElement &&other) noexcept
			: TagName(std::move(other.TagName))
			, Properties(std::move(other.Properties))
			, Children(std::move(other.Children))
		{ }

		virtual ~HtmlElement() = default;

		template<typename _Ty, typename ...Args>
		auto AddProperty(Args && ...args) {
			auto prop = std::make_shared<_Ty>(args...);
			return Properties.emplace_back(prop);
		}

		template<typename _Ty, typename ...Args>
		auto AddChild(Args && ...args) {
			auto child = std::make_shared<_Ty>(args...);
			return Children.emplace_back(child);
		}

		friend std::ostream &operator<<(std::ostream &os, HtmlElement const &self) {
			self.Print(os);
			return os;
		}

		virtual	operator std::string() const {
			std::ostringstream oss;
			oss << *this;
			return oss.str();
		}
	protected:
		virtual void Print(std::ostream &os) const {
			// start element
			os << "<" << TagName;
			for (auto const &prop : Properties) {
				os << " " << *prop;
			}
			os << ">";

			// element body — 直接流式输出子元素，避免临时 string 分配
			for (auto const &child : Children) {
				os << *child;
			}

			// end element
			os << "</" << TagName << ">";
		}
	};

	class HtmlDocumentHead : public HtmlElement {
	public:
		explicit HtmlDocumentHead() {
			TagName = "head";
		}
	};

	class HtmlDocumentBody : public HtmlElement {
	public:
		explicit HtmlDocumentBody() {
			TagName = "body";
		}
	};

	class HtmlDocument : public HtmlElement {
	public:
		std::shared_ptr<HtmlDocumentHead> Head;
		std::shared_ptr<HtmlDocumentBody> Body;

		explicit HtmlDocument() {
			TagName = "html";
			auto _ = AddProperty<KeyValueProperty>("lang", "zh-CN");

			Head = std::make_shared<HtmlDocumentHead>();
			Children.emplace_back(Head);

			Body = std::make_shared<HtmlDocumentBody>();
			Children.emplace_back(Body);
		}

	private:
		virtual	void Print(std::ostream &os) const {
			os << "<!DOCTYPE html>\n";
			HtmlElement::Print(os);
		}
	};

	class PureTextElement : public HtmlElement {
		std::string _text;
	public:
		explicit PureTextElement(std::string text)
			: _text(std::move(text)) { }

		PureTextElement(PureTextElement &&other) noexcept
			: _text(std::move(other._text)) { }

		virtual	operator std::string() const override {
			return Detail::HtmlEncode(_text);
		}

	protected:
		virtual void Print(std::ostream &os) const override {
			os << Detail::HtmlEncode(_text);
		}
	};

	class DivElement : public HtmlElement {
	public:
		explicit DivElement() {
			TagName = "div";
		}
	};

	template <size_t Cols>
	class TableElement : public HtmlElement {
		// ── 单元格类型 ──
		class HeaderCell : public HtmlElement {
		public:
			explicit HeaderCell(std::string const &text) {
				TagName = "th";
				AddChild<PureTextElement>(text);
			}
		};

		class DataCell : public HtmlElement {
		public:
			explicit DataCell(std::string const &text) {
				TagName = "td";
				AddChild<PureTextElement>(text);
			}
		};

		// ── 行类型 ──
		template<typename TCell>
		class Row : public HtmlElement {
		public:
			static_assert(std::is_same_v<TCell, HeaderCell> || std::is_same_v<TCell, DataCell>,
				"TCell must be HeaderCell or DataCell");

			explicit Row(std::array<std::string, Cols> const &cells) {
				TagName = "tr";
				for (size_t i = 0; i < Cols; ++i) {
					AddChild<TCell>(cells[i]);
				}
			}
		};

		// ── 表体 ──
		class TableBody : public HtmlElement {
		public:
			explicit TableBody(std::array<std::string, Cols> const &headers) {
				TagName = "tbody";
				AddChild<Row<HeaderCell>>(headers);
			}

			void AppendRow(std::array<std::string, Cols> const &cells) {
				AddChild<Row<DataCell>>(cells);
			}
		};

		std::shared_ptr<TableBody> _body;

	public:
		explicit TableElement(std::array<std::string, Cols> const &headers) {
			TagName = "table";
			_body = std::make_shared<TableBody>(headers);
			Children.emplace_back(_body);
		}

		void AppendDataRow(std::array<std::string, Cols> const &cells) {
			_body->AppendRow(cells);
		}
	};

	class MetaElement : public HtmlElement {
	public:
		explicit MetaElement() {
			TagName = "meta";
		}

		explicit MetaElement(std::string_view charset) {
			TagName = "meta";
			AddProperty<KeyValueProperty>("charset", charset);
		}

		explicit MetaElement(std::string_view name, std::string_view content) {
			TagName = "meta";
			AddProperty<KeyValueProperty>("name", name);
			AddProperty<KeyValueProperty>("content", content);
		}
	};

	class TitleElement : public HtmlElement {
	public:
		explicit TitleElement(std::string const &title) {
			TagName = "title";
			auto _ = AddChild<PureTextElement>(title);
		}
	};

	class StyleElement : public HtmlElement {
	public:
		explicit StyleElement(std::string const &stylesheet) {
			TagName = "style";
			auto _ = AddProperty<KeyValueProperty>("type", "text/css");
			auto __ = AddChild<PureTextElement>(stylesheet);
		}
	};

}
