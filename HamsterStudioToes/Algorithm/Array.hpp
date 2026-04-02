
#pragma once

namespace HamsterStudio
{
	template<typename _Ty>
	class List {
	public:
		using Postion = size_t;
	public:
		List() { }
		~List() { }

	public:
		virtual bool IsNull() const = 0;

		virtual int InsertPre(Postion pos, _Ty data) = 0;
		virtual int InsertPost(Postion pos, _Ty data) = 0;

		virtual int DelIndex(Postion pos) = 0;
		virtual int DelValue(_Ty value) = 0;

		virtual int LocateIndex(_Ty value) = 0;
		virtual int LocatePosition(_Ty value) = 0;

	};

	template<typename _Ty>
	class IEnumerable {
	public:
		using EnumCallback = void(*)(_Ty const &t);
	public:
		virtual void Each(EnumCallback callee) const = 0;
	};

	template<typename _Ty>
	class Stack {
	public:
		using Postion = size_t;
	public:
		Stack() { }
		~Stack() { }
	public:
		virtual bool IsNull() const = 0;

		virtual void Push(_Ty const &data) = 0;
		virtual void Pop() = 0;

		virtual _Ty Top() = 0;

	};

	template<typename _Ty>
	class StackImpl : public Stack<_Ty>, public IEnumerable<_Ty> {
	public:
		StackImpl(size_t cap) :m_data(nullptr)
		{
			m_Max = cap;
			m_data = new _Ty[cap];
			m_Top = -1;
		}
		~StackImpl() { }
	public:
		virtual bool IsNull() const
		{
			return m_data == nullptr || m_Top == -1;
		}

		virtual void Push(_Ty const &data)
		{
			if (m_Top == m_Max - 1) {
				m_data = new(m_data) _Ty[m_Max *= 2];
			}
			m_Top++;
			m_data[m_Top] = data;
		}

		virtual void Pop()
		{
			if (m_Top == -1) return;
			m_Top--;
		}

		virtual _Ty Top()
		{
			_Ty t = m_data[m_Top];
			Pop();
			return t;
		}

		virtual void Each(IEnumerable<_Ty>::EnumCallback callee) const
		{
			if (IsNull()) return;
			for (int i = 0; i <= m_Top; i++) {
				callee(m_data[i]);
			}
		}

	private:
		_Ty *m_data;
		size_t m_Top;
		Stack<_Ty>::Postion m_Max;
	};

	class StackLinkImpl {

	};

	template<typename _Ty>
	class Queue {
	public:
		Queue() { }
		~Queue() { }
	public:
		virtual bool IsNull() const = 0;

		virtual void En() = 0;
		virtual void De() = 0;

		virtual _Ty Front() = 0;

	};

	class ITree {
	public:
		virtual bool IsNull() const = 0;
		virtual void PreOrder() const = 0;
		virtual void InOrder() const = 0;
		virtual void PostOrder() const = 0;
		virtual void LevelOrder() const = 0;
	};

	template< typename _Ty>
	class Tree : public ITree {
		using TreePtr = std::shared_ptr<Tree>;
		using TreeDataPtr = std::shared_ptr<_Ty>;
	public:
		Tree() :left(nullptr), right(nullptr), val(nullptr) { }
		//Tree(_Ty const &t) :left(nullptr), right(nullptr), val(std::make_shared<_Ty>(t)) { }
		Tree(TreeDataPtr const &t) :left(nullptr), right(nullptr), val(t) { }
		//Tree(Tree<_Ty> const &t) : left(t.left), right(t.right), val(t.val) { }
		~Tree() { }

		TreeDataPtr Value() const
		{
			return val;
		}

		TreeDataPtr Value(TreeDataPtr const &ptr)
		{
			return val = ptr;
		}

		TreePtr Left() const
		{
			return left;
		}

		TreePtr Left(TreePtr const &l)
		{
			return (left = l);
		}

		TreePtr Left(TreeDataPtr const &l)
		{
			if (left == nullptr) { left = std::make_shared<Tree>(l); }
			else { left->val = l; }
			return Left();
		}

		TreePtr Right() const
		{
			return right;
		}

		TreePtr Right(TreePtr const &r)
		{
			return (right = r);
		}

		TreePtr Right(TreeDataPtr const &r)
		{
			if (right == nullptr) { right = std::make_shared<Tree>(r); }
			else { right->val = r; }
			return Right();
		}

		virtual bool IsNull() const
		{
			return  left == nullptr && right == nullptr && val == nullptr;
		}

		virtual void PreOrder() const
		{
			return PreOrder([] (TreeDataPtr sp) {
				Algorithm::IO::Print(*sp);
				});
		}

		virtual void InOrder() const
		{

		}

		virtual void PostOrder() const
		{

		}

		virtual void LevelOrder() const
		{

		}

		template< typename _Pred>
		void PreOrderR(_Pred pred) const
		{
			if (IsNull()) return;
			pred(val);
			if (left != nullptr) left->PreOrder(pred);
			if (right != nullptr) right->PreOrder(pred);
		}

		template< typename _Pred>
		void PreOrder(_Pred pred) const
		{
			if (IsNull()) return;
			pred(val);
			PreOrderTree(left, pred);
			PreOrderTree(right, pred);
		}

		template< typename _Pred>
		void InOrder(_Pred pred) const
		{

		}

		template< typename _Pred>
		void PostOrder(_Pred pred) const
		{

		}

		template< typename _Pred>
		void LevelOrder(_Pred pred) const
		{

		}

	public:
		template< typename _Pred>
		static void PreOrderTree(TreePtr const &t, _Pred pred)
		{
			TreePtr cur = t;
			//StackImpl<std::shared_ptr<Tree>> nodes;
			std::stack<TreePtr> nodes;
			while (!cur->IsNull()) {
				pred(cur->val);
				if (cur->left != nullptr) {
					if (cur->right != nullptr) {
						nodes.push(cur->right);
					}
					cur = cur->left;
				}
				else if (cur->right != nullptr) {
					cur = cur->right;
				}
				else if (nodes.empty()) {
					break;
				}
				else {
					cur = nodes.top(); nodes.pop();
				}
			}
		}

	private:
		TreePtr left, right;
		TreeDataPtr val;
	};

}