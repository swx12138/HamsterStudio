#pragma once

#include <string>
#include <sstream>
#include <vector>
#include <memory>
#include <array>
#include <type_traits>

#define HTML_DOCUMENT_NAMESPACE utils::html

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

		virtual void Print(std::ostream &os) const {
			os << _key << "=\"" << _value << "\"";
		}

		virtual	operator std::string() const
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
		virtual ~HtmlElement() { }

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

			// element body
			for (auto const &child : Children) {
				os << child->operator std::string();
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
			:_text(text) { }

		friend std::ostream &operator << (std::ostream &os, PureTextElement const &self) {
			os << self._text;
			return os;
		}

		virtual	operator std::string() const {
			return  _text;
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
		class TableBodyElement : public HtmlElement {
			// th
			class HeaderElement : public HtmlElement {
			public:
				explicit HeaderElement(std::string const &header) {
					TagName = "th";
					auto _ = AddChild<PureTextElement>(header);
				}
			};

			// td
			class DataElement : public HtmlElement {
			public:
				explicit DataElement(std::string const &header) {
					TagName = "td";
					auto _ = AddChild<PureTextElement>(header);
				}
			};

			// tr
			template<typename Ty>
			class RowElement : public HtmlElement {
			public:
				explicit RowElement(std::array<std::string, Cols> const &headers) {
					TagName = "tr";

					static_assert(std::is_same_v<DataElement, Ty> || std::is_same_v<HeaderElement, Ty>,
						"must be DataElement or HeaderElement");
					for (int i = 0; i < Cols; i++) {
						auto _ = AddChild<Ty>(headers[i]);
					}
				}
			};

		public:
			explicit TableBodyElement(std::array<std::string, Cols> const &headers) {
				TagName = "tbody";
				auto _ = AddChild<RowElement<HeaderElement>>(headers);
			}

		public:
			void AppendDataRow(std::array<std::string, Cols> const &cells) {
				auto _ = AddChild<RowElement<DataElement>>(cells);
			}
		};

		std::shared_ptr<TableBodyElement> TableBody;
	public:
		explicit TableElement(std::array<std::string, Cols> const &headers)
		{
			TagName = "table";
			TableBody = std::make_shared < TableBodyElement>(headers);
			Children.emplace_back(TableBody);
		}

	public:

		void AppendDataRow(std::array<std::string, Cols> const &cells) {
			TableBody->AppendDataRow(cells);
		}

	};

	class MetaElement : public HtmlElement {
	public:
		explicit MetaElement() {
			TagName = "meta";
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
