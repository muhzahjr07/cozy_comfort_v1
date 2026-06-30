import { useState, useEffect } from 'react'

const API_BASE = 'http://localhost:5035/api'

export default function App() {
  // ── Authentication State ──────────────────────────────────────────────────
  const [token, setToken] = useState(localStorage.getItem('token') || '')
  const [user, setUser] = useState(() => {
    try { return JSON.parse(localStorage.getItem('cozy_user') || 'null') } catch { return null }
  })
  const [authEmail, setAuthEmail] = useState('')
  const [authPassword, setAuthPassword] = useState('')
  const [authMode, setAuthMode] = useState('login') // 'login' or 'register'
  
  // Registration additional fields
  const [regFirstName, setRegFirstName] = useState('')
  const [regLastName, setRegLastName] = useState('')
  const [regRole, setRegRole] = useState('Seller')
  const [regSellerName, setRegSellerName] = useState('')
  const [regSellerAddress, setRegSellerAddress] = useState('')
  const [regDistributorId, setRegDistributorId] = useState('')
  const [regDistributorName, setRegDistributorName] = useState('')
  const [regDistributorPhone, setRegDistributorPhone] = useState('')

  // ── Application States ─────────────────────────────────────────────────────
  const [activeTab, setActiveTab] = useState('')
  const [toasts, setToasts] = useState([])
  const [loading, setLoading] = useState(false)
  const [blankets, setBlankets] = useState([])
  const [distributors, setDistributors] = useState([])
  
  // Seller States
  const [sellerOrders, setSellerOrders] = useState([])
  const [queryHistory, setQueryHistory] = useState([])
  const [stockRequest, setStockRequest] = useState({ blanketId: '', quantity: 1 })
  const [stockResult, setStockResult] = useState(null)
  const [orderForm, setOrderForm] = useState({ customerName: '', customerEmail: '', deliveryAddress: '', notes: '' })
  
  // Shopping Cart States
  const [cart, setCart] = useState(() => {
    try {
      return JSON.parse(localStorage.getItem('cozy_cart') || '[]')
    } catch {
      return []
    }
  })
  const [cartStockResults, setCartStockResults] = useState([])
  const [cartStockAggregated, setCartStockAggregated] = useState(null)
  const [cartOrderForm, setCartOrderForm] = useState({ customerName: '', customerEmail: '', deliveryAddress: '', notes: '' })

  // ── Customer Store States ─────────────────────────────────────────────────
  const [showStore, setShowStore] = useState(false)
  const [storeTab, setStoreTab] = useState('home') // 'home' | 'browse' | 'cart' | 'checkout'
  const [storeBlankets, setStoreBlankets] = useState([])
  const [storeSearch, setStoreSearch] = useState('')
  const [customerCart, setCustomerCart] = useState(() => {
    try { return JSON.parse(localStorage.getItem('customer_cart') || '[]') } catch { return [] }
  })
  const [customerForm, setCustomerForm] = useState({ name: '', email: '', address: '', notes: '' })
  const [customerOrderResult, setCustomerOrderResult] = useState(null)

  
  // Distributor States
  const [distributorOrders, setDistributorOrders] = useState([])
  const [distributorStocks, setDistributorStocks] = useState([])
  const [distributorProdOrders, setDistributorProdOrders] = useState([])
  const [distributorProfile, setDistributorProfile] = useState(null)
  const [editingStock, setEditingStock] = useState(null) // { blanketId, stockQuantity, costPrice }

  // Manufacturer States
  const [mfgOrders, setMfgOrders] = useState([])
  const [mfgProdOrders, setMfgProdOrders] = useState([])
  const [mfgStats, setMfgStats] = useState(null)
  const [editingBlanket, setEditingBlanket] = useState(null) // { blanketId, name, description, material, size, basePrice, imageUrl, initialStock, dailyProductionCapacity, leadTimeDays }
  const [mfgStockEdit, setMfgStockEdit] = useState(null) // { blanketId, stockQuantity, dailyProductionCapacity, leadTimeDays }

  // ── Toast Helper ──────────────────────────────────────────────────────────
  const showToast = (message, type = 'success') => {
    const id = Date.now()
    setToasts(prev => [...prev, { id, message, type }])
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id))
    }, 4000)
  }

  // ── Fetch Wrapper ──────────────────────────────────────────────────────────
  const apiFetch = async (endpoint, options = {}) => {
    const headers = {
      'Content-Type': 'application/json',
      ...options.headers
    }
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    try {
      const res = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers
      })
      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}))
        throw new Error(errorData.message || `HTTP error ${res.status}`)
      }
      const data = await res.json()
      return data
    } catch (err) {
      showToast(err.message, 'error')
      throw err
    }
  }

  // ── Persist token & restore session on mount ─────────────────────────────
  useEffect(() => {
    if (token) {
      localStorage.setItem('token', token)
    } else {
      localStorage.removeItem('token')
      localStorage.removeItem('cozy_user')
      setUser(null)
    }
  }, [token])

  // Set default tab when user is first loaded
  useEffect(() => {
    if (user && !activeTab) {
      if (user.role === 'Manufacturer') setActiveTab('mfg-dashboard')
      else if (user.role === 'Distributor') setActiveTab('dist-orders')
      else setActiveTab('seller-catalog')
    }
  }, [user])

  useEffect(() => {
    localStorage.setItem('cozy_cart', JSON.stringify(cart))
  }, [cart])

  useEffect(() => {
    localStorage.setItem('customer_cart', JSON.stringify(customerCart))
  }, [customerCart])

  // Load store blankets whenever store is opened
  useEffect(() => {
    if (showStore && storeBlankets.length === 0) {
      fetch(`${API_BASE}/blankets?pageSize=100`)
        .then(r => r.json())
        .then(d => setStoreBlankets(d.data?.data || []))
        .catch(() => {})
    }
  }, [showStore])

  // ── Load Data based on User Role & Tab ───────────────────────────────────
  useEffect(() => {
    if (!user) return

    if (user.role === 'Seller') {
      loadBlankets()
      loadDistributors()
      if (activeTab === 'seller-orders') loadSellerOrders()
      if (activeTab === 'seller-history') loadQueryHistory()
    } else if (user.role === 'Distributor') {
      if (activeTab === 'dist-orders') loadDistributorOrders()
      if (activeTab === 'dist-stock') loadDistributorStock()
      if (activeTab === 'dist-production') loadDistributorProdOrders()
      if (activeTab === 'dist-profile') loadDistributorProfile()
    } else if (user.role === 'Manufacturer') {
      if (activeTab === 'mfg-dashboard') loadMfgStats()
      if (activeTab === 'mfg-blankets') loadBlankets()
      if (activeTab === 'mfg-orders') loadMfgOrders()
      if (activeTab === 'mfg-production') loadMfgProdOrders()
    }
  }, [user, activeTab])

  // ── Actions ────────────────────────────────────────────────────────────────
  const handleLogout = () => {
    setToken('')
    setUser(null)
    setActiveTab('')
    localStorage.removeItem('token')
    localStorage.removeItem('cozy_user')
    showToast('Logged out successfully.')
  }

  const handleLogin = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const data = await apiFetch('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: authEmail, password: authPassword })
      })
      if (data.success && data.data) {
        const profile = data.data
        setToken(profile.token)
        setUser(profile)
        localStorage.setItem('cozy_user', JSON.stringify(profile))
        // Navigate to role-appropriate default tab
        if (profile.role === 'Manufacturer') setActiveTab('mfg-dashboard')
        else if (profile.role === 'Distributor') setActiveTab('dist-orders')
        else setActiveTab('seller-catalog')
        showToast(`Welcome back, ${profile.firstName}!`)
      } else {
        showToast(data.message || 'Login failed', 'error')
      }
    } catch (err) {
      // Handled by apiFetch
    } finally {
      setLoading(false)
    }
  }

  const handleRegister = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const body = {
        email: authEmail,
        password: authPassword,
        firstName: regFirstName,
        lastName: regLastName,
        role: regRole,
        sellerName: regRole === 'Seller' ? regSellerName : null,
        sellerShippingAddress: regRole === 'Seller' ? regSellerAddress : null,
        assignedDistributorId: regRole === 'Seller' && regDistributorId ? parseInt(regDistributorId) : null,
        distributorName: regRole === 'Distributor' ? regDistributorName : null,
        distributorContactPhone: regRole === 'Distributor' ? regDistributorPhone : null
      }

      const data = await apiFetch('/auth/register', {
        method: 'POST',
        body: JSON.stringify(body)
      })

      if (data.success) {
        showToast('Registration successful! Please login.')
        setAuthMode('login')
      } else {
        showToast(data.message || 'Registration failed', 'error')
      }
    } catch (err) {
      // Handled by apiFetch
    } finally {
      setLoading(false)
    }
  }

  // ── Customer Store Operations ──────────────────────────────────────────────
  const addToCustomerCart = (blanket) => {
    setCustomerCart(prev => {
      const existing = prev.find(item => item.blanketId === blanket.blanketId)
      if (existing) {
        showToast(`Quantity updated for ${blanket.name}.`)
        return prev.map(item => item.blanketId === blanket.blanketId ? { ...item, quantity: item.quantity + 1 } : item)
      }
      showToast(`${blanket.name} added to cart! 🛍️`)
      return [...prev, { ...blanket, quantity: 1 }]
    })
  }

  const removeFromCustomerCart = (blanketId) => {
    setCustomerCart(prev => prev.filter(item => item.blanketId !== blanketId))
    showToast('Item removed from cart.')
  }

  const updateCustomerCartQty = (blanketId, delta) => {
    setCustomerCart(prev => prev.map(item => {
      if (item.blanketId === blanketId) {
        const newQty = Math.max(1, item.quantity + delta)
        return { ...item, quantity: newQty }
      }
      return item
    }))
  }

  const handleCustomerCheckout = async (e) => {
    e.preventDefault()
    if (customerCart.length === 0) return
    setLoading(true)
    try {
      const body = {
        customerName: customerForm.name,
        customerEmail: customerForm.email,
        deliveryAddress: customerForm.address,
        notes: customerForm.notes,
        items: customerCart.map(item => ({
          blanketId: item.blanketId,
          quantity: item.quantity,
          unitPrice: item.basePrice
        }))
      }
      const res = await fetch(`${API_BASE}/orders/guest`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
      })
      const data = await res.json()
      if (data.success) {
        setCustomerOrderResult(data.data)
        setCustomerCart([])
        setCustomerForm({ name: '', email: '', address: '', notes: '' })
        setStoreTab('checkout')
        showToast('🎉 Your order has been placed!')
      } else {
        showToast(data.message || 'Order failed. Please try again.', 'error')
      }
    } catch (err) {
      showToast('Unable to place order. Please try again.', 'error')
    } finally {
      setLoading(false)
    }
  }

  // ── Cart Operations ────────────────────────────────────────────────────────
  const addToCart = (blanket) => {
    setCart(prev => {
      const existing = prev.find(item => item.blanketId === blanket.blanketId)
      if (existing) {
        showToast(`Increased quantity of ${blanket.name} in cart.`)
        return prev.map(item =>
          item.blanketId === blanket.blanketId
            ? { ...item, quantity: item.quantity + 1 }
            : item
        )
      }
      showToast(`Added ${blanket.name} to cart.`)
      return [...prev, { ...blanket, quantity: 1 }]
    })
  }

  const removeFromCart = (blanketId) => {
    setCart(prev => prev.filter(item => item.blanketId !== blanketId))
    showToast('Removed item from cart.')
  }

  const updateCartQuantity = (blanketId, delta) => {
    setCart(prev => prev.map(item => {
      if (item.blanketId === blanketId) {
        const newQty = Math.max(1, item.quantity + delta)
        return { ...item, quantity: newQty }
      }
      return item
    }))
  }

  const handleBulkStockCheck = async (e) => {
    e.preventDefault()
    if (cart.length === 0) return
    setLoading(true)
    try {
      const results = []
      for (const item of cart) {
        const data = await apiFetch('/stockcheck/query', {
          method: 'POST',
          body: JSON.stringify({
            sellerId: user.sellerId,
            blanketId: item.blanketId,
            quantity: item.quantity
          })
        })
        results.push(data.data)
      }
      
      setCartStockResults(results)
      
      const totalAmount = results.reduce((sum, r) => sum + (r.priceQuoted * r.quantityRequested), 0)
      const maxLeadTime = results.reduce((max, r) => Math.max(max, r.calculatedLeadTimeDays), 0)
      
      setCartStockAggregated({
        totalAmount,
        maxLeadTime,
        estimatedDeliveryDate: new Date(Date.now() + maxLeadTime * 24 * 60 * 60 * 1000).toISOString()
      })
      showToast('Bulk stock verification route calculated.')
    } catch (err) {
      setCartStockResults([])
      setCartStockAggregated(null)
    } finally {
      setLoading(false)
    }
  }

  const handlePlaceCartOrder = async (e) => {
    e.preventDefault()
    if (cart.length === 0 || !cartStockAggregated) return
    setLoading(true)
    try {
      const itemsPayload = cart.map(item => {
        const stockRes = cartStockResults.find(r => r.blanketId === item.blanketId)
        return {
          blanketId: item.blanketId,
          quantity: item.quantity,
          unitPrice: stockRes ? stockRes.priceQuoted : item.basePrice
        }
      })
      
      const body = {
        sellerId: user.sellerId,
        customerName: cartOrderForm.customerName,
        customerEmail: cartOrderForm.customerEmail,
        deliveryAddress: cartOrderForm.deliveryAddress,
        notes: cartOrderForm.notes,
        items: itemsPayload
      }
      
      await apiFetch('/orders', {
        method: 'POST',
        body: JSON.stringify(body)
      })
      
      showToast('Store bulk order placed successfully!')
      setCart([])
      setCartStockResults([])
      setCartStockAggregated(null)
      setCartOrderForm({ customerName: '', customerEmail: '', deliveryAddress: '', notes: '' })
      setActiveTab('seller-orders')
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  // ── Sellers Logic ─────────────────────────────────────────────────────────
  const loadBlankets = async () => {
    try {
      const data = await apiFetch('/blankets?pageSize=100')
      setBlankets(data.data?.data || [])
    } catch (err) {}
  }

  const loadDistributors = async () => {
    try {
      const data = await apiFetch('/distributors')
      setDistributors(data.data || [])
    } catch (err) {}
  }

  const loadSellerOrders = async () => {
    if (!user?.sellerId) return
    try {
      const data = await apiFetch(`/orders/seller/${user.sellerId}`)
      setSellerOrders(data.data || [])
    } catch (err) {}
  }

  const loadQueryHistory = async () => {
    try {
      const data = await apiFetch(`/stockcheck/history?sellerId=${user.sellerId}`)
      setQueryHistory(data.data || [])
    } catch (err) {}
  }

  const handleStockCheck = async (e) => {
    e.preventDefault()
    if (!stockRequest.blanketId) {
      showToast('Please select a blanket model.', 'error')
      return
    }
    setLoading(true)
    try {
      const data = await apiFetch('/stockcheck/query', {
        method: 'POST',
        body: JSON.stringify({
          sellerId: user.sellerId,
          blanketId: parseInt(stockRequest.blanketId),
          quantity: parseInt(stockRequest.quantity)
        })
      })
      setStockResult(data.data)
      showToast('Stock verification route calculated.')
    } catch (err) {
      setStockResult(null)
    } finally {
      setLoading(false)
    }
  }

  const handlePlaceOrder = async (e) => {
    e.preventDefault()
    if (!stockResult) return
    setLoading(true)
    try {
      const body = {
        sellerId: user.sellerId,
        customerName: orderForm.customerName,
        customerEmail: orderForm.customerEmail,
        deliveryAddress: orderForm.deliveryAddress,
        notes: orderForm.notes,
        items: [
          {
            blanketId: stockResult.blanketId,
            quantity: stockResult.quantityRequested,
            unitPrice: stockResult.priceQuoted
          }
        ]
      }
      await apiFetch('/orders', {
        method: 'POST',
        body: JSON.stringify(body)
      })
      showToast('Order placed successfully!')
      setStockResult(null)
      setOrderForm({ customerName: '', customerEmail: '', deliveryAddress: '', notes: '' })
      setActiveTab('seller-orders')
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  // ── Distributor Logic ─────────────────────────────────────────────────────
  const loadDistributorOrders = async () => {
    if (!user?.distributorId) return
    try {
      const data = await apiFetch(`/orders/distributor/${user.distributorId}`)
      setDistributorOrders(data.data || [])
    } catch (err) {}
  }

  const loadDistributorStock = async () => {
    if (!user?.distributorId) return
    try {
      const data = await apiFetch(`/distributors/${user.distributorId}/stock`)
      setDistributorStocks(data.data || [])
    } catch (err) {}
  }

  const loadDistributorProdOrders = async () => {
    if (!user?.distributorId) return
    try {
      const data = await apiFetch(`/orders/production?distributorId=${user.distributorId}`)
      setDistributorProdOrders(data.data || [])
    } catch (err) {}
  }

  const loadDistributorProfile = async () => {
    if (!user?.distributorId) return
    try {
      const data = await apiFetch(`/distributors/${user.distributorId}`)
      setDistributorProfile(data.data)
    } catch (err) {}
  }

  const handleUpdateStock = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await apiFetch(`/distributors/${user.distributorId}/stock`, {
        method: 'PUT',
        body: JSON.stringify({
          blanketId: editingStock.blanketId,
          stockQuantity: parseInt(editingStock.stockQuantity),
          costPrice: parseFloat(editingStock.costPrice)
        })
      })
      showToast('Distributor stock level updated successfully.')
      setEditingStock(null)
      loadDistributorStock()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await apiFetch(`/distributors/${user.distributorId}`, {
        method: 'PUT',
        body: JSON.stringify({
          name: distributorProfile.name,
          contactEmail: distributorProfile.contactEmail,
          contactPhone: distributorProfile.contactPhone,
          shippingCost: parseFloat(distributorProfile.shippingCost),
          logisticsSpeedDays: parseInt(distributorProfile.logisticsSpeedDays)
        })
      })
      showToast('Profile and shipping metrics saved.')
      loadDistributorProfile()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleShipOrder = async (orderId) => {
    setLoading(true)
    try {
      await apiFetch(`/orders/${orderId}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status: 'Shipped' })
      })
      showToast('Order marked as Shipped to Customer.')
      loadDistributorOrders()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  // ── Manufacturer Logic ────────────────────────────────────────────────────
  const loadMfgStats = async () => {
    try {
      const data = await apiFetch('/orders/dashboard')
      setMfgStats(data.data)
    } catch (err) {}
  }

  const loadMfgOrders = async () => {
    try {
      const data = await apiFetch('/orders')
      setMfgOrders(data.data || [])
    } catch (err) {}
  }

  const loadMfgProdOrders = async () => {
    try {
      const data = await apiFetch('/orders/production')
      setMfgProdOrders(data.data || [])
    } catch (err) {}
  }

  const handleSaveBlanket = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const method = editingBlanket.blanketId ? 'PUT' : 'POST'
      const url = editingBlanket.blanketId ? `/blankets/${editingBlanket.blanketId}` : '/blankets'
      await apiFetch(url, {
        method,
        body: JSON.stringify(editingBlanket)
      })
      showToast('Blanket model catalog entry saved.')
      setEditingBlanket(null)
      loadBlankets()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleDeleteBlanket = async (id) => {
    if (!confirm('Are you sure you want to delete this blanket model?')) return
    setLoading(true)
    try {
      await apiFetch(`/blankets/${id}`, { method: 'DELETE' })
      showToast('Blanket deactivated successfully.')
      loadBlankets()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleUpdateMfgStock = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await apiFetch(`/blankets/${mfgStockEdit.blanketId}/manufacturer-stock`, {
        method: 'PUT',
        body: JSON.stringify({
          stockQuantity: parseInt(mfgStockEdit.stockQuantity),
          dailyProductionCapacity: parseInt(mfgStockEdit.dailyProductionCapacity),
          leadTimeDays: parseInt(mfgStockEdit.leadTimeDays)
        })
      })
      showToast('Manufacturer stock and production lines updated.')
      setMfgStockEdit(null)
      loadBlankets()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleCompleteProduction = async (prodOrderId) => {
    setLoading(true)
    try {
      await apiFetch(`/orders/production/${prodOrderId}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status: 'Completed' })
      })
      showToast('Production run completed. Inventory replenished and parent order updated.')
      loadMfgProdOrders()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  const handleDeliverOrder = async (orderId) => {
    setLoading(true)
    try {
      await apiFetch(`/orders/${orderId}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status: 'Delivered' })
      })
      showToast('Order confirmed Delivered.')
      if (activeTab === 'mfg-orders') loadMfgOrders()
    } catch (err) {
    } finally {
      setLoading(false)
    }
  }

  // ── Render Helpers ────────────────────────────────────────────────────────
  const getRouteLabel = (route) => {
    switch (route) {
      case 'SellerStock': return 'Fulfillment: Seller local stock (0 day lead time)'
      case 'DistributorStock': return 'Fulfillment: Distributor partner stock'
      case 'ManufacturerStock': return 'Fulfillment: Manufacturer central warehouse'
      case 'ProductionRequired': return 'Fulfillment: Production run required'
      default: return route
    }
  }

  const getStatusBadge = (status) => {
    switch (status) {
      case 'Delivered':
        return <span className="badge success"><span className="badge-dot" />Delivered</span>
      case 'Shipped':
      case 'ShippedToDistributor':
        return <span className="badge info"><span className="badge-dot" />Shipped</span>
      case 'AwaitingProduction':
      case 'Scheduled':
        return <span className="badge warning"><span className="badge-dot" />Awaiting Production</span>
      case 'Processing':
        return <span className="badge purple"><span className="badge-dot" />Processing</span>
      default:
        return <span className="badge muted"><span className="badge-dot" />{status}</span>
    }
  }

  // ── Render Auth View ──────────────────────────────────────────────────────
  // ── Customer Storefront (no login required) ───────────────────────────────
  if (showStore) {
    const filteredBlankets = storeBlankets.filter(b =>
      !storeSearch || b.name?.toLowerCase().includes(storeSearch.toLowerCase()) ||
      b.material?.toLowerCase().includes(storeSearch.toLowerCase()) ||
      b.size?.toLowerCase().includes(storeSearch.toLowerCase())
    )
    const customerCartTotal = customerCart.reduce((s, i) => s + i.basePrice * i.quantity, 0)
    const customerCartCount = customerCart.reduce((s, i) => s + i.quantity, 0)

    return (
      <div className="store-shell">
        {/* Top nav */}
        <nav className="store-nav">
          <div className="store-nav-brand">🛏️ Cozy Comfort Store</div>
          <div className="store-nav-links">
            <button className={`store-nav-link ${storeTab === 'home' ? 'active' : ''}`} onClick={() => setStoreTab('home')}>🏠 Home</button>
            <button className={`store-nav-link ${storeTab === 'browse' ? 'active' : ''}`} onClick={() => setStoreTab('browse')}>🛍️ Browse</button>
            <button
              className={`store-nav-link ${storeTab === 'cart' ? 'active' : ''}`}
              onClick={() => setStoreTab('cart')}
            >
              🛒 Cart {customerCartCount > 0 && <span className="cart-badge">{customerCartCount}</span>}
            </button>
          </div>
          <div className="store-nav-actions">
            {user ? (
              <button className="btn btn-secondary btn-sm" onClick={() => setShowStore(false)}>⬅ Dashboard</button>
            ) : (
              <button className="btn btn-primary btn-sm" onClick={() => setShowStore(false)}>🔑 Staff Login</button>
            )}
          </div>
        </nav>

        {/* Toast */}
        <div className="toast-container">
          {toasts.map(t => (
            <div key={t.id} className={`toast ${t.type === 'error' ? 'error' : 'success'}`}>
              <span>{t.type === 'error' ? '❌' : '✅'}</span>
              <span>{t.message}</span>
            </div>
          ))}
        </div>

        {/* Home tab */}
        {storeTab === 'home' && (
          <>
            <section className="store-hero">
              <div className="store-hero-emoji">🛏️</div>
              <h1>Wrap Yourself in Warmth</h1>
              <p>Premium handcrafted blankets, made with love and the finest materials. Free delivery on orders over $150.</p>
              <div className="store-hero-actions">
                <button className="btn btn-primary btn-lg" onClick={() => setStoreTab('browse')}>Shop Now</button>
                <button className="btn btn-secondary btn-lg" onClick={() => setStoreTab('browse')}>View Collection</button>
              </div>
            </section>

            <div className="category-strip">
              {[{emoji:'🌡️',label:'Heated'},{emoji:'🍃',label:'Organic'},{emoji:'❄️',label:'Cooling'},{emoji:'✨',label:'Fleece'},{emoji:'🏠',label:'Throw'},{emoji:'🎁',label:'Gift Sets'}].map(c => (
                <div key={c.label} className="category-chip" onClick={() => { setStoreSearch(c.label); setStoreTab('browse'); }}>
                  <span className="chip-emoji">{c.emoji}</span>
                  {c.label}
                </div>
              ))}
            </div>

            <div className="store-content">
              <div className="store-promo-banner">
                <div>
                  <h3>Winter Warmth Sale 🔥</h3>
                  <p>Up to 30% off selected blankets. Limited time offer — order today!</p>
                </div>
                <button className="store-promo-btn" onClick={() => setStoreTab('browse')}>Shop the Sale</button>
              </div>

              <h2 style={{ marginBottom: '20px', fontSize: '22px', fontWeight: 700 }}>Featured Blankets</h2>
              <div className="store-product-grid">
                {storeBlankets.slice(0, 4).map(b => (
                  <div key={b.blanketId} className="store-product-card">
                    <div className="store-product-img-wrap">
                      {b.imageUrl ? (
                        <img src={b.imageUrl} alt={b.name} className="store-product-img" onError={e => e.target.style.display='none'} />
                      ) : (
                        <div className="store-product-img-placeholder">🛏️</div>
                      )}
                    </div>
                    <div className="store-product-body">
                      <div className="store-product-name">{b.name}</div>
                      <div className="store-product-desc">{b.description}</div>
                      <div className="store-product-tags">
                        <span className="badge purple">{b.material}</span>
                        <span className="badge info">{b.size}</span>
                      </div>
                      <div className="store-product-footer">
                        <div className="store-product-price">${b.basePrice?.toFixed(2)}</div>
                        <button className="store-add-btn" onClick={() => addToCustomerCart(b)}>Add to Cart</button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </>
        )}

        {/* Browse tab */}
        {storeTab === 'browse' && (
          <div className="store-content">
            <h2 style={{ marginBottom: '20px', fontSize: '22px', fontWeight: 700 }}>All Blankets</h2>
            <div className="store-search-bar">
              <div className="store-search-wrap">
                <span className="store-search-icon">🔍</span>
                <input
                  type="text"
                  className="store-search-input"
                  placeholder="Search by name, material, or size..."
                  value={storeSearch}
                  onChange={e => setStoreSearch(e.target.value)}
                />
              </div>
              {storeSearch && (
                <button className="btn btn-secondary btn-sm" onClick={() => setStoreSearch('')}>Clear</button>
              )}
            </div>
            {filteredBlankets.length === 0 ? (
              <div className="empty-state">
                <div className="empty-icon">🔍</div>
                <h3>No blankets found</h3>
                <p>Try a different search term.</p>
              </div>
            ) : (
              <div className="store-product-grid">
                {filteredBlankets.map(b => (
                  <div key={b.blanketId} className="store-product-card">
                    <div className="store-product-img-wrap">
                      {b.imageUrl ? (
                        <img src={b.imageUrl} alt={b.name} className="store-product-img" onError={e => e.target.style.display='none'} />
                      ) : (
                        <div className="store-product-img-placeholder">🛏️</div>
                      )}
                    </div>
                    <div className="store-product-body">
                      <div className="store-product-name">{b.name}</div>
                      <div className="store-product-desc">{b.description}</div>
                      <div className="store-product-tags">
                        <span className="badge purple">{b.material}</span>
                        <span className="badge info">{b.size}</span>
                      </div>
                      <div className="store-product-footer">
                        <div className="store-product-price">${b.basePrice?.toFixed(2)}</div>
                        <button className="store-add-btn" onClick={() => addToCustomerCart(b)}>Add to Cart</button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Cart tab */}
        {storeTab === 'cart' && (
          <div className="customer-cart-page">
            {customerCart.length === 0 ? (
              <div className="customer-cart-empty">
                <div className="empty-icon">🛒</div>
                <h3 style={{ fontSize: '24px', marginBottom: '10px' }}>Your cart is empty</h3>
                <p style={{ color: 'var(--text-secondary)', marginBottom: '24px' }}>Browse our collection and add some cozy blankets!</p>
                <button className="btn btn-primary btn-lg" onClick={() => setStoreTab('browse')}>Start Shopping</button>
              </div>
            ) : (
              <div className="checkout-grid">
                <div>
                  <div className="card">
                    <h3 style={{ marginBottom: '20px', fontSize: '18px' }}>Your Items ({customerCartCount})</h3>
                    <div className="cart-table">
                      {customerCart.map(item => (
                        <div key={item.blanketId} style={{ display: 'flex', alignItems: 'center', gap: '16px', padding: '14px 0', borderBottom: '1px solid var(--border)' }}>
                          <div style={{ width: 64, height: 64, borderRadius: 'var(--radius-sm)', overflow: 'hidden', background: 'var(--bg-base)', flexShrink: 0, border: '1px solid var(--border)' }}>
                            {item.imageUrl ? <img src={item.imageUrl} alt={item.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }} onError={e => e.target.style.display='none'} /> : <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', fontSize: 28 }}>🛏️</div>}
                          </div>
                          <div style={{ flex: 1 }}>
                            <div style={{ fontWeight: 700, marginBottom: 4 }}>{item.name}</div>
                            <div style={{ display: 'flex', gap: 6 }}>
                              <span className="badge purple">{item.material}</span>
                              <span className="badge info">{item.size}</span>
                            </div>
                          </div>
                          <div className="quantity-control">
                            <button className="quantity-btn" onClick={() => updateCustomerCartQty(item.blanketId, -1)}>-</button>
                            <span className="quantity-value">{item.quantity}</span>
                            <button className="quantity-btn" onClick={() => updateCustomerCartQty(item.blanketId, 1)}>+</button>
                          </div>
                          <div style={{ fontFamily: 'Outfit', fontWeight: 800, fontSize: 16, color: 'var(--primary)', minWidth: 80, textAlign: 'right' }}>
                            ${(item.basePrice * item.quantity).toFixed(2)}
                          </div>
                          <button className="remove-item-btn" onClick={() => removeFromCustomerCart(item.blanketId)}>Remove</button>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div className="card mt-4">
                    <h3 style={{ marginBottom: '20px', fontSize: '18px' }}>Delivery Details</h3>
                    <form onSubmit={handleCustomerCheckout}>
                      <div className="grid-2">
                        <div className="form-group">
                          <label className="form-label">Your Name</label>
                          <input type="text" className="form-control" placeholder="Jane Doe" required
                            value={customerForm.name} onChange={e => setCustomerForm(p => ({ ...p, name: e.target.value }))} />
                        </div>
                        <div className="form-group">
                          <label className="form-label">Email Address</label>
                          <input type="email" className="form-control" placeholder="jane@email.com" required
                            value={customerForm.email} onChange={e => setCustomerForm(p => ({ ...p, email: e.target.value }))} />
                        </div>
                      </div>
                      <div className="form-group">
                        <label className="form-label">Delivery Address</label>
                        <input type="text" className="form-control" placeholder="123 Main St, City, State ZIP" required
                          value={customerForm.address} onChange={e => setCustomerForm(p => ({ ...p, address: e.target.value }))} />
                      </div>
                      <div className="form-group">
                        <label className="form-label">Order Notes (optional)</label>
                        <textarea className="form-control" placeholder="Gift message, special instructions..."
                          value={customerForm.notes} onChange={e => setCustomerForm(p => ({ ...p, notes: e.target.value }))} />
                      </div>
                      <button type="submit" className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: '8px' }} disabled={loading}>
                        {loading ? <span className="loading-spinner" /> : `Place Order — $${customerCartTotal.toFixed(2)}`}
                      </button>
                    </form>
                  </div>
                </div>

                <div className="order-summary-card">
                  <h3 style={{ marginBottom: '20px', fontSize: '18px' }}>Order Summary</h3>
                  {customerCart.map(item => (
                    <div key={item.blanketId} className="summary-row">
                      <span>{item.name} ×{item.quantity}</span>
                      <span>${(item.basePrice * item.quantity).toFixed(2)}</span>
                    </div>
                  ))}
                  <div className="summary-row">
                    <span>Shipping</span>
                    <span style={{ color: 'var(--success)', fontWeight: 600 }}>{customerCartTotal >= 150 ? 'Free 🎉' : '$9.99'}</span>
                  </div>
                  <div className="summary-row total">
                    <span>Total</span>
                    <span>${(customerCartTotal + (customerCartTotal >= 150 ? 0 : 9.99)).toFixed(2)}</span>
                  </div>
                  {customerCartTotal < 150 && (
                    <p style={{ fontSize: '12px', color: 'var(--text-muted)', marginTop: '10px', textAlign: 'center' }}>
                      Add ${(150 - customerCartTotal).toFixed(2)} more for free shipping!
                    </p>
                  )}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Checkout success tab */}
        {storeTab === 'checkout' && customerOrderResult && (
          <div className="store-content">
            <div className="order-success-card">
              <div className="success-icon">🎉</div>
              <h2>Order Confirmed!</h2>
              <p>Thank you for shopping with Cozy Comfort.</p>
              <p>Your order has been placed and is being processed.</p>
              <div className="order-ref">{customerOrderResult.orderReference}</div>
              <p style={{ marginTop: '8px' }}>A confirmation will be sent to <strong>{customerOrderResult.customerEmail}</strong></p>
              <p style={{ marginTop: '4px', color: 'var(--text-muted)', fontSize: '13px' }}>Estimated delivery: {customerOrderResult.estimatedDeliveryDate ? new Date(customerOrderResult.estimatedDeliveryDate).toLocaleDateString() : '5-10 business days'}</p>
              <div style={{ marginTop: '28px', display: 'flex', gap: '12px', justifyContent: 'center' }}>
                <button className="btn btn-primary" onClick={() => { setStoreTab('browse'); setCustomerOrderResult(null); }}>Continue Shopping</button>
              </div>
            </div>
          </div>
        )}
        <Footer onStaffPortal={() => { setShowStore(false); setAuthMode('login'); }} />
      </div>
    )
  }

  if (!user) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-logo">
            <div className="logo-icon">🛏️</div>
            <h1>Cozy Comfort</h1>
            <p>Service-Oriented Order & Supply Chain System</p>
          </div>

          <div className="auth-tabs">
            <button
              className={`auth-tab ${authMode === 'login' ? 'active' : ''}`}
              onClick={() => setAuthMode('login')}
            >
              Login
            </button>
            <button
              className={`auth-tab ${authMode === 'register' ? 'active' : ''}`}
              onClick={() => setAuthMode('register')}
            >
              Register
            </button>
          </div>

          {authMode === 'login' ? (
            <form onSubmit={handleLogin}>
              <div className="form-group">
                <label className="form-label">Email Address</label>
                <input
                  type="email"
                  className="form-control"
                  placeholder="name@cozycomfort.com"
                  required
                  value={authEmail}
                  onChange={e => setAuthEmail(e.target.value)}
                />
              </div>
              <div className="form-group">
                <label className="form-label">Password</label>
                <input
                  type="password"
                  className="form-control"
                  placeholder="••••••••"
                  required
                  value={authPassword}
                  onChange={e => setAuthPassword(e.target.value)}
                />
              </div>
              <button type="submit" className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: '16px' }} disabled={loading}>
                {loading ? <span className="loading-spinner" /> : 'Log In'}
              </button>
              
              <div className="text-center text-muted" style={{ marginTop: '20px', fontSize: '12px' }}>
                <p>Try Seeded Accounts (Password: <strong>password123</strong>):</p>
                <p style={{ marginTop: '4px' }}>
                  Seller: <code>owner@nestandhome.com</code> | Distributor: <code>contact@warmroutes.com</code> | Mfg: <code>admin@cozycomfort.com</code>
                </p>
              </div>
            </form>
          ) : (
            <form onSubmit={handleRegister}>
              <div className="grid-2">
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="John"
                    required
                    value={regFirstName}
                    onChange={e => setRegFirstName(e.target.value)}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Doe"
                    required
                    value={regLastName}
                    onChange={e => setRegLastName(e.target.value)}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Email Address</label>
                <input
                  type="email"
                  className="form-control"
                  placeholder="john.doe@gmail.com"
                  required
                  value={authEmail}
                  onChange={e => setAuthEmail(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Password (Min 6 chars)</label>
                <input
                  type="password"
                  className="form-control"
                  placeholder="••••••••"
                  required
                  value={authPassword}
                  onChange={e => setAuthPassword(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Select Account Role</label>
                <select className="form-control" value={regRole} onChange={e => setRegRole(e.target.value)}>
                  <option value="Seller">Seller (Retailer / Outlet)</option>
                  <option value="Distributor">Distributor (Logistics Partner)</option>
                  <option value="Manufacturer">Manufacturer (Cozy Comfort)</option>
                </select>
              </div>

              {regRole === 'Seller' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Store / Seller Name</label>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="My Blanket Outlet"
                      required
                      value={regSellerName}
                      onChange={e => setRegSellerName(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Store Address</label>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="123 Cozy Way, Portland OR"
                      required
                      value={regSellerAddress}
                      onChange={e => setRegSellerAddress(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Assign Distributor Partner</label>
                    <select
                      className="form-control"
                      required
                      value={regDistributorId}
                      onChange={e => setRegDistributorId(e.target.value)}
                    >
                      <option value="">Select a logistics partner...</option>
                      {distributors.map(d => (
                        <option key={d.distributorId} value={d.distributorId}>{d.name} ({d.shippingCost} USD shipping)</option>
                      ))}
                    </select>
                  </div>
                </>
              )}

              {regRole === 'Distributor' && (
                <>
                  <div className="form-group">
                    <label className="form-label">Distributor Company Name</label>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="Warm Logistics Ltd"
                      required
                      value={regDistributorName}
                      onChange={e => setRegDistributorName(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Contact Phone</label>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="+1-555-123-4567"
                      required
                      value={regDistributorPhone}
                      onChange={e => setRegDistributorPhone(e.target.value)}
                    />
                  </div>
                </>
              )}

              <button type="submit" className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: '16px' }} disabled={loading}>
                {loading ? <span className="loading-spinner" /> : 'Register Account'}
              </button>
            </form>
          )}

          {/* Visit Store strip */}
          <div className="auth-store-strip">
            <p>🛍️ Want to browse our <strong>blanket collection</strong> as a customer?</p>
            <button className="btn btn-primary btn-sm" onClick={() => setShowStore(true)}>Visit Store</button>
          </div>
        </div>
        
        {/* Toast notifications */}
        <div className="toast-container">
          {toasts.map(t => (
            <div key={t.id} className={`toast ${t.type === 'error' ? 'error' : 'success'}`}>
              <span>{t.type === 'error' ? '❌' : '✅'}</span>
              <span>{t.message}</span>
            </div>
          ))}
        </div>
        <Footer onVisitStore={() => setShowStore(true)} />
      </div>
    )
  }

  // ── Main Shell ─────────────────────────────────────────────────────────────
  return (
    <div className="app-shell">
      {/* Sidebar navigation */}
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-logo">Cozy Comfort</div>
          <div className="brand-sub">SOC System</div>
        </div>

        <div className={`sidebar-role-badge ${user.role.toLowerCase()}`}>
          {user.role} Account
        </div>

        <nav className="sidebar-nav">
          {user.role === 'Seller' && (
            <>
              <div className="nav-section-label">Seller Operations</div>
              <div className={`nav-item ${activeTab === 'seller-catalog' ? 'active' : ''}`} onClick={() => setActiveTab('seller-catalog')}>
                <span className="nav-icon">📦</span> Blanket Catalog
              </div>
              <div className={`nav-item ${activeTab === 'seller-stockcheck' ? 'active' : ''}`} onClick={() => setActiveTab('seller-stockcheck')}>
                <span className="nav-icon">🔍</span> Supply Chain Stock Check
              </div>
              <div className={`nav-item ${activeTab === 'seller-cart' ? 'active' : ''}`} onClick={() => setActiveTab('seller-cart')}>
                <span className="nav-icon">🛒</span> Shopping Cart
                {cart.length > 0 && (
                  <span className="cart-badge">{cart.reduce((sum, item) => sum + item.quantity, 0)}</span>
                )}
              </div>
              <div className={`nav-item ${activeTab === 'seller-orders' ? 'active' : ''}`} onClick={() => setActiveTab('seller-orders')}>
                <span className="nav-icon">📦</span> Placed Orders
              </div>
              <div className={`nav-item ${activeTab === 'seller-history' ? 'active' : ''}`} onClick={() => setActiveTab('seller-history')}>
                <span className="nav-icon">📜</span> Stock Query History
              </div>
            </>
          )}

          {user.role === 'Distributor' && (
            <>
              <div className="nav-section-label">Distributor Portal</div>
              <div className={`nav-item ${activeTab === 'dist-orders' ? 'active' : ''}`} onClick={() => setActiveTab('dist-orders')}>
                <span className="nav-icon">📥</span> Received Orders
              </div>
              <div className={`nav-item ${activeTab === 'dist-stock' ? 'active' : ''}`} onClick={() => setActiveTab('dist-stock')}>
                <span className="nav-icon">🏢</span> Manage Local Stock
              </div>
              <div className={`nav-item ${activeTab === 'dist-production' ? 'active' : ''}`} onClick={() => setActiveTab('dist-production')}>
                <span className="nav-icon">🏗️</span> Manufacturer Orders
              </div>
              <div className={`nav-item ${activeTab === 'dist-profile' ? 'active' : ''}`} onClick={() => setActiveTab('dist-profile')}>
                <span className="nav-icon">⚙️</span> Logistics & Costs
              </div>
            </>
          )}

          {user.role === 'Manufacturer' && (
            <>
              <div className="nav-section-label">Mfg Console</div>
              <div className={`nav-item ${activeTab === 'mfg-dashboard' ? 'active' : ''}`} onClick={() => setActiveTab('mfg-dashboard')}>
                <span className="nav-icon">📊</span> Stats Dashboard
              </div>
              <div className={`nav-item ${activeTab === 'mfg-blankets' ? 'active' : ''}`} onClick={() => setActiveTab('mfg-blankets')}>
                <span className="nav-icon">🛏️</span> Blanket Models
              </div>
              <div className={`nav-item ${activeTab === 'mfg-orders' ? 'active' : ''}`} onClick={() => setActiveTab('mfg-orders')}>
                <span className="nav-icon">📃</span> System Orders
              </div>
              <div className={`nav-item ${activeTab === 'mfg-production' ? 'active' : ''}`} onClick={() => setActiveTab('mfg-production')}>
                <span className="nav-icon">🏭</span> Production Runs
              </div>
            </>
          )}
          {/* Store link for all roles */}
          <div className="divider" style={{ margin: '8px 0' }} />
          <div className="nav-section-label">Customer Facing</div>
          <div className="nav-item" onClick={() => setShowStore(true)}>
            <span className="nav-icon">🏪</span> Customer Store
          </div>
        </nav>

        <div className="sidebar-footer">
          <div className="user-card">
            <div className="user-avatar">{user.firstName[0]}{user.lastName[0]}</div>
            <div className="user-info">
              <div className="user-name">{user.firstName} {user.lastName}</div>
              <div className="user-role">{user.email}</div>
            </div>
            <button className="logout-btn" onClick={handleLogout} title="Log Out">
              🚪
            </button>
          </div>
        </div>
      </aside>

      <main className="main-area">
        <header className="header">
          <div className="flex-1">
            <h3 style={{ fontFamily: 'Outfit', fontWeight: 600 }}>
              {activeTab === 'seller-catalog' && 'Blanket Collection'}
              {activeTab === 'seller-stockcheck' && 'Supply Chain Verification'}
              {activeTab === 'seller-cart' && 'Store Shopping Cart & Checkout'}
              {activeTab === 'seller-orders' && 'Store Orders'}
              {activeTab === 'seller-history' && 'Query History'}
              {activeTab === 'dist-orders' && 'Assigned Orders'}
              {activeTab === 'dist-stock' && 'Inventory Control'}
              {activeTab === 'dist-production' && 'Production Status'}
              {activeTab === 'dist-profile' && 'Shipping & Logistics Settings'}
              {activeTab === 'mfg-dashboard' && 'Manufacturer Dashboard'}
              {activeTab === 'mfg-blankets' && 'Manage Blanket Models'}
              {activeTab === 'mfg-orders' && 'All Orders'}
              {activeTab === 'mfg-production' && 'Factory Production Line'}
            </h3>
          </div>
          {loading && <span className="loading-spinner" />}
        </header>

        <div className="page-content">
          {/* ──────────────────────────────────────────────────────────────────
              SELLER VIEWS
              ────────────────────────────────────────────────────────────────── */}
          {user.role === 'Seller' && (
            <>
              {activeTab === 'seller-catalog' && (
                <div>
                  <div className="page-header">
                    <h1>Blanket Catalog</h1>
                    <p>Displaying models available for custom customer order fulfillment.</p>
                  </div>
                  
                  {blankets.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">🛏️</div>
                      <h3>No blanket models available</h3>
                      <p>Blanket specifications will appear once the manufacturer adds them.</p>
                    </div>
                  ) : (
                    <div className="blanket-grid">
                      {blankets.map(b => (
                        <div key={b.blanketId} className="blanket-card" style={{ cursor: 'default' }}>
                          <div className="blanket-image-wrap" style={{ cursor: 'pointer' }} onClick={() => {
                            setStockRequest({ blanketId: b.blanketId.toString(), quantity: 1 })
                            setStockResult(null)
                            setActiveTab('seller-stockcheck')
                          }}>
                            <img src={b.imageUrl} className="blanket-image" alt={b.name} onError={e => {
                              e.target.style.display = 'none'
                            }} />
                            <div className="blanket-image-placeholder">🛏️</div>
                          </div>
                          <div className="blanket-body">
                            <div style={{ cursor: 'pointer' }} onClick={() => {
                              setStockRequest({ blanketId: b.blanketId.toString(), quantity: 1 })
                              setStockResult(null)
                              setActiveTab('seller-stockcheck')
                            }}>
                              <div className="blanket-name">{b.name}</div>
                              <div className="blanket-desc">{b.description}</div>
                              <div className="blanket-meta">
                                <span className="badge info">{b.material}</span>
                                <span className="badge purple" style={{ marginLeft: '4px' }}>{b.size}</span>
                              </div>
                              <div className="blanket-price">${b.basePrice}</div>
                            </div>
                            <button className="catalog-add-btn" onClick={() => addToCart(b)}>
                              🛒 Add to Cart
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'seller-stockcheck' && (
                <div>
                  <div className="page-header">
                    <h1>Supply Chain Stock Check</h1>
                    <p>Verify stock level route across all 4 tiers in the supply chain to calculate delivery lead times.</p>
                  </div>

                  <div className="grid-2">
                    <div className="card">
                      <h3 className="mb-4">Select Blanket Specifications</h3>
                      <form onSubmit={handleStockCheck}>
                        <div className="form-group">
                          <label className="form-label">Blanket Model</label>
                          <select
                            className="form-control"
                            value={stockRequest.blanketId}
                            onChange={e => {
                              setStockRequest(prev => ({ ...prev, blanketId: e.target.value }))
                              setStockResult(null)
                            }}
                          >
                            <option value="">Choose a model...</option>
                            {blankets.map(b => (
                              <option key={b.blanketId} value={b.blanketId}>{b.name} ({b.size} - {b.material})</option>
                            ))}
                          </select>
                        </div>
                        <div className="form-group">
                          <label className="form-label">Quantity Needed</label>
                          <input
                            type="number"
                            min="1"
                            className="form-control"
                            value={stockRequest.quantity}
                            onChange={e => {
                              setStockRequest(prev => ({ ...prev, quantity: Math.max(1, parseInt(e.target.value)) }))
                              setStockResult(null)
                            }}
                          />
                        </div>
                        <button type="submit" className="btn btn-primary" style={{ width: '100%', marginTop: '12px' }} disabled={loading}>
                          Verify Inventory Path
                        </button>
                      </form>
                    </div>

                    {stockResult && (
                      <div className="card highlighted">
                        <h3 className="mb-4">Route Calculation Results</h3>
                        
                        <div style={{ marginBottom: '20px' }}>
                          <p style={{ fontSize: '13px', color: 'var(--text-secondary)' }}>Fulfillment Route:</p>
                          <h4 className="text-primary mt-4" style={{ fontSize: '18px' }}>
                            {getRouteLabel(stockResult.routeTaken)}
                          </h4>
                        </div>

                        <div className="grid-2 mb-4">
                          <div>
                            <span className="text-muted" style={{ fontSize: '12px' }}>UNIT PRICE</span>
                            <h4 style={{ fontSize: '20px' }}>${stockResult.priceQuoted.toFixed(2)}</h4>
                          </div>
                          <div>
                            <span className="text-muted" style={{ fontSize: '12px' }}>ESTIMATED TIME</span>
                            <h4 style={{ fontSize: '20px' }}>{stockResult.calculatedLeadTimeDays} days</h4>
                          </div>
                        </div>

                        <p className="text-muted" style={{ fontSize: '12px' }}>
                          Delivery partner: <strong>{stockResult.distributorName}</strong> | Est. Arrival: {new Date(stockResult.estimatedDeliveryDate).toLocaleDateString()}
                        </p>
                      </div>
                    )}
                  </div>

                  {stockResult && (
                    <div style={{ marginTop: '28px' }}>
                      <h3 className="mb-4">Tier Routing Visualization</h3>
                      
                      <div className="supply-chain-viz">
                        {/* Tier 1: Seller */}
                        <div className={`supply-tier ${stockResult.routeTaken === 'SellerStock' ? 'active' : 'inactive'}`}>
                          <div className="supply-tier-icon">🏪</div>
                          <div className="supply-tier-name">Tier 1: Seller</div>
                          <div className="supply-tier-value">{stockResult.sellerStockLevel}</div>
                          <div className="supply-tier-desc">Local inventory. Instant delivery (0 days)</div>
                        </div>
                        <div className={`supply-arrow ${['DistributorStock', 'ManufacturerStock', 'ProductionRequired'].includes(stockResult.routeTaken) ? 'active' : ''}`}>➔</div>

                        {/* Tier 2: Distributor */}
                        <div className={`supply-tier ${stockResult.routeTaken === 'DistributorStock' ? 'active' : 'inactive'}`}>
                          <div className="supply-tier-icon">🚚</div>
                          <div className="supply-tier-name">Tier 2: Distributor</div>
                          <div className="supply-tier-value">{stockResult.distributorStockLevel}</div>
                          <div className="supply-tier-desc">Partner stock. Local shipping</div>
                        </div>
                        <div className={`supply-arrow ${['ManufacturerStock', 'ProductionRequired'].includes(stockResult.routeTaken) ? 'active' : ''}`}>➔</div>

                        {/* Tier 3: Manufacturer */}
                        <div className={`supply-tier ${stockResult.routeTaken === 'ManufacturerStock' ? 'active' : 'inactive'}`}>
                          <div className="supply-tier-icon">🏢</div>
                          <div className="supply-tier-name">Tier 3: Factory Warehouse</div>
                          <div className="supply-tier-value">{stockResult.manufacturerStockLevel}</div>
                          <div className="supply-tier-desc">Central depot. Mfg lead time + logistics</div>
                        </div>
                        <div className={`supply-arrow ${stockResult.routeTaken === 'ProductionRequired' ? 'active' : ''}`}>➔</div>

                        {/* Tier 4: Production */}
                        <div className={`supply-tier ${stockResult.routeTaken === 'ProductionRequired' ? 'active' : 'inactive'}`}>
                          <div className="supply-tier-icon">🏭</div>
                          <div className="supply-tier-name">Tier 4: Production Run</div>
                          <div className="supply-tier-value">Capacity: {stockResult.dailyProductionCapacity}/day</div>
                          <div className="supply-tier-desc">Factory line queue. Extended duration</div>
                        </div>
                      </div>

                      {/* Customer Order form */}
                      <div className="card">
                        <h3 className="mb-4">Place Customer Order</h3>
                        <form onSubmit={handlePlaceOrder}>
                          <div className="grid-2">
                            <div className="form-group">
                              <label className="form-label">Customer Name</label>
                              <input
                                type="text"
                                className="form-control"
                                placeholder="Alice Smith"
                                required
                                value={orderForm.customerName}
                                onChange={e => setOrderForm(prev => ({ ...prev, customerName: e.target.value }))}
                              />
                            </div>
                            <div className="form-group">
                              <label className="form-label">Customer Email</label>
                              <input
                                type="email"
                                className="form-control"
                                placeholder="alice@example.com"
                                required
                                value={orderForm.customerEmail}
                                onChange={e => setOrderForm(prev => ({ ...prev, customerEmail: e.target.value }))}
                              />
                            </div>
                          </div>
                          <div className="form-group">
                            <label className="form-label">Delivery Address</label>
                            <input
                              type="text"
                              className="form-control"
                              placeholder="77 Cozy Lane, Seattle WA 98101"
                              required
                              value={orderForm.deliveryAddress}
                              onChange={e => setOrderForm(prev => ({ ...prev, deliveryAddress: e.target.value }))}
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Order Notes / Customizations</label>
                            <textarea
                              className="form-control"
                              placeholder="Add special requests..."
                              value={orderForm.notes}
                              onChange={e => setOrderForm(prev => ({ ...prev, notes: e.target.value }))}
                            />
                          </div>
                          <button type="submit" className="btn btn-success btn-lg mt-4" style={{ width: '100%' }}>
                            Confirm & Place Order for ${ (stockResult.priceQuoted * stockResult.quantityRequested).toFixed(2) }
                          </button>
                        </form>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'seller-orders' && (
                <div>
                  <div className="page-header">
                    <h1>Store Placed Orders</h1>
                    <p>Track delivery status and routing details of blanket orders placed by your store.</p>
                  </div>

                  {sellerOrders.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">🛒</div>
                      <h3>No orders placed yet</h3>
                      <p>Run a stock verification route and place an order to get started.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Reference</th>
                              <th>Customer</th>
                              <th>Details</th>
                              <th>Shipping Address</th>
                              <th>Logistics Partner</th>
                              <th>Date</th>
                              <th>Total</th>
                              <th>Status</th>
                            </tr>
                          </thead>
                          <tbody>
                            {sellerOrders.map(o => (
                              <tr key={o.orderId}>
                                <td><strong className="text-primary">{o.orderReference}</strong></td>
                                <td>
                                  <div>{o.customerName}</div>
                                  <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>{o.customerEmail}</div>
                                </td>
                                <td>
                                  {o.orderItems.map(oi => (
                                    <div key={oi.orderItemId}>
                                      {oi.blanketName} (x{oi.quantity})
                                    </div>
                                  ))}
                                </td>
                                <td><span style={{ fontSize: '13px' }}>{o.deliveryAddress}</span></td>
                                <td>{o.distributorName || 'None'}</td>
                                <td>{new Date(o.orderDate).toLocaleDateString()}</td>
                                <td><strong>${o.totalAmount.toFixed(2)}</strong></td>
                                <td>{getStatusBadge(o.status)}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'seller-history' && (
                <div>
                  <div className="page-header">
                    <h1>Stock Query History</h1>
                    <p>Trace log of all supply chain route checks executed by this store.</p>
                  </div>

                  {queryHistory.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">📜</div>
                      <h3>No queries executed</h3>
                      <p>Use the Stock Check tool to query the supply chain path.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Query Date</th>
                              <th>Blanket Model</th>
                              <th>Qty Requested</th>
                              <th>Seller Stock</th>
                              <th>Distributor Stock</th>
                              <th>Mfg Stock</th>
                              <th>Calculated Lead Days</th>
                              <th>Estimated Delivery</th>
                              <th>Price Quoted</th>
                            </tr>
                          </thead>
                          <tbody>
                            {queryHistory.map(h => (
                              <tr key={h.supplyChainQueryId}>
                                <td>{new Date(h.queryDate).toLocaleString()}</td>
                                <td><strong>{h.blanketName}</strong></td>
                                <td>{h.quantityRequested}</td>
                                <td>{h.sellerStockLevel}</td>
                                <td>{h.distributorStockLevel}</td>
                                <td>{h.manufacturerStockLevel}</td>
                                <td>{h.calculatedLeadTimeDays} days</td>
                                <td>{new Date(h.estimatedDeliveryDate).toLocaleDateString()}</td>
                                <td><strong>${h.distributorCostQuoted.toFixed(2)}</strong></td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'seller-cart' && (
                <div>
                  <div className="page-header">
                    <h1>Store Shopping Cart</h1>
                    <p>Review customer blanket selections, verify stock levels across the supply chain, and place bulk orders.</p>
                  </div>

                  {cart.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">🛒</div>
                      <h3>Your shopping cart is empty</h3>
                      <p>Browse the catalog and add blankets to the cart to begin.</p>
                      <button className="btn btn-primary mt-4" onClick={() => setActiveTab('seller-catalog')}>
                        Browse Catalog
                      </button>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table className="cart-table">
                          <thead>
                            <tr>
                              <th>Product</th>
                              <th>Specifications</th>
                              <th>Unit Price</th>
                              <th>Quantity</th>
                              <th>Total</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {cart.map(item => (
                              <tr key={item.blanketId}>
                                <td>
                                  <div className="cart-item-info">
                                    <img src={item.imageUrl} className="cart-item-image" alt={item.name} onError={e => {
                                      e.target.style.display = 'none';
                                    }} />
                                    <div>
                                      <strong>{item.name}</strong>
                                    </div>
                                  </div>
                                </td>
                                <td>
                                  <span className="badge info">{item.material}</span>
                                  <span className="badge purple" style={{ marginLeft: '4px' }}>{item.size}</span>
                                </td>
                                <td>${item.basePrice.toFixed(2)}</td>
                                <td>
                                  <div className="quantity-control">
                                    <button className="quantity-btn" onClick={() => updateCartQuantity(item.blanketId, -1)}>-</button>
                                    <span className="quantity-value">{item.quantity}</span>
                                    <button className="quantity-btn" onClick={() => updateCartQuantity(item.blanketId, 1)}>+</button>
                                  </div>
                                </td>
                                <td><strong>${(item.basePrice * item.quantity).toFixed(2)}</strong></td>
                                <td>
                                  <button className="remove-item-btn" onClick={() => removeFromCart(item.blanketId)}>Remove</button>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}

                  {cart.length > 0 && (
                    <div className="checkout-grid">
                      <div>
                        <div className="card">
                          <h3 className="mb-4">1. Supply Chain Verification</h3>
                          <p className="text-secondary mb-4" style={{ fontSize: '13px' }}>
                            Before submitting, we must verify the stock availability paths for all blankets in your cart. This computes the delivery routes (Seller {'>'} Distributor {'>'} Manufacturer {'>'} Production) and quotes final prices.
                          </p>
                          
                          <button className="btn btn-primary" onClick={handleBulkStockCheck} disabled={loading}>
                            {loading ? <span className="loading-spinner" /> : 'Run Bulk Stock Check'}
                          </button>

                          {cartStockResults.length > 0 && (
                            <div className="bulk-stock-result">
                              <h4 className="mb-2" style={{ fontSize: '14px', color: 'var(--primary)' }}>Verification Path Results:</h4>
                              {cartStockResults.map(r => (
                                <div key={r.blanketId} className="bulk-stock-item-route">
                                  <span>{r.blanketName} (x{r.quantityRequested})</span>
                                  <div>
                                    <span className="badge success" style={{ marginRight: '6px' }}>{getRouteLabel(r.RouteTaken || r.routeTaken)}</span>
                                    <strong>${(r.priceQuoted * r.quantityRequested).toFixed(2)}</strong>
                                  </div>
                                </div>
                              ))}
                            </div>
                          )}
                        </div>

                        {cartStockAggregated && (
                          <div className="card mt-4">
                            <h3 className="mb-4">2. Customer Checkout & Delivery Details</h3>
                            <form onSubmit={handlePlaceCartOrder}>
                              <div className="grid-2">
                                <div className="form-group">
                                  <label className="form-label">Customer Name</label>
                                  <input
                                    type="text"
                                    className="form-control"
                                    placeholder="Jane Doe"
                                    required
                                    value={cartOrderForm.customerName}
                                    onChange={e => setCartOrderForm(prev => ({ ...prev, customerName: e.target.value }))}
                                  />
                                </div>
                                <div className="form-group">
                                  <label className="form-label">Customer Email</label>
                                  <input
                                    type="email"
                                    className="form-control"
                                    placeholder="jane.doe@example.com"
                                    required
                                    value={cartOrderForm.customerEmail}
                                    onChange={e => setCartOrderForm(prev => ({ ...prev, customerEmail: e.target.value }))}
                                  />
                                </div>
                              </div>
                              <div className="form-group">
                                <label className="form-label">Delivery Address</label>
                                <input
                                  type="text"
                                  className="form-control"
                                  placeholder="456 Cozy Boulevard, Portland, OR 97201"
                                  required
                                  value={cartOrderForm.deliveryAddress}
                                  onChange={e => setCartOrderForm(prev => ({ ...prev, deliveryAddress: e.target.value }))}
                                />
                              </div>
                              <div className="form-group">
                                <label className="form-label">Special Order Notes</label>
                                <textarea
                                  className="form-control"
                                  placeholder="Special requests or custom blanket length specifications..."
                                  value={cartOrderForm.notes}
                                  onChange={e => setCartOrderForm(prev => ({ ...prev, notes: e.target.value }))}
                                />
                              </div>
                              <button type="submit" className="btn btn-success btn-lg mt-4" style={{ width: '100%' }} disabled={loading}>
                                Confirm & Place Bulk Order
                              </button>
                            </form>
                          </div>
                        )}
                      </div>

                      <div className="order-summary-card">
                        <h3 className="mb-4" style={{ fontSize: '18px' }}>Order Summary</h3>
                        
                        <div className="summary-row">
                          <span>Items Subtotal</span>
                          <span>${cart.reduce((sum, item) => sum + (item.basePrice * item.quantity), 0).toFixed(2)}</span>
                        </div>

                        {cartStockAggregated ? (
                          <>
                            <div className="summary-row">
                              <span>Shipping & Handling</span>
                              <span>${(cartStockResults[0]?.priceQuoted ? (cartStockResults.reduce((sum, r) => sum + (r.priceQuoted * r.quantityRequested), 0) - cart.reduce((sum, item) => sum + (item.basePrice * item.quantity), 0)) : 0).toFixed(2)}</span>
                            </div>
                            <div className="summary-row">
                              <span>Estimated Lead Time</span>
                              <span>{cartStockAggregated.maxLeadTime} days</span>
                            </div>
                            <div className="summary-row">
                              <span>Est. Delivery Date</span>
                              <span>{new Date(cartStockAggregated.estimatedDeliveryDate).toLocaleDateString()}</span>
                            </div>
                            <div className="summary-row total">
                              <span>Grand Total</span>
                              <span>${cartStockAggregated.totalAmount.toFixed(2)}</span>
                            </div>
                          </>
                        ) : (
                          <div className="text-muted text-center mt-4" style={{ fontSize: '12px' }}>
                            Please run the Supply Chain Verification check to calculate final logistics costs, routes, and lead times.
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              )}
            </>
          )}

          {/* ──────────────────────────────────────────────────────────────────
              DISTRIBUTOR VIEWS
              ────────────────────────────────────────────────────────────────── */}
          {user.role === 'Distributor' && (
            <>
              {activeTab === 'dist-orders' && (
                <div>
                  <div className="page-header">
                    <h1>Received Orders</h1>
                    <p>Manage and dispatch blanket orders requested by your assigned sellers.</p>
                  </div>

                  {distributorOrders.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">📥</div>
                      <h3>No received orders</h3>
                      <p>Orders will display here once sellers submit request fulfillments.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Reference</th>
                              <th>Retailer Seller</th>
                              <th>Customer Details</th>
                              <th>Order Items</th>
                              <th>Delivery Destination</th>
                              <th>Date Placed</th>
                              <th>Fulfillment Status</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {distributorOrders.map(o => (
                              <tr key={o.orderId}>
                                <td><strong className="text-primary">{o.orderReference}</strong></td>
                                <td>{o.sellerName}</td>
                                <td>
                                  <div>{o.customerName}</div>
                                  <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>{o.customerEmail}</div>
                                </td>
                                <td>
                                  {o.orderItems.map(oi => (
                                    <div key={oi.orderItemId}>
                                      {oi.blanketName} (x{oi.quantity})
                                    </div>
                                  ))}
                                </td>
                                <td><span style={{ fontSize: '13px' }}>{o.deliveryAddress}</span></td>
                                <td>{new Date(o.orderDate).toLocaleDateString()}</td>
                                <td>{getStatusBadge(o.status)}</td>
                                <td>
                                  {o.status === 'Processing' && (
                                    <button className="btn btn-primary btn-sm" onClick={() => handleShipOrder(o.orderId)}>
                                      Dispatch Shipping
                                    </button>
                                  )}
                                  {o.status === 'ShippedToDistributor' && (
                                    <button className="btn btn-success btn-sm" onClick={() => handleShipOrder(o.orderId)}>
                                      Accept & Ship
                                    </button>
                                  )}
                                  {o.status === 'AwaitingProduction' && (
                                    <span className="text-muted" style={{ fontSize: '12px' }}>Waiting for Factory</span>
                                  )}
                                  {o.status === 'Shipped' && (
                                    <span className="text-success" style={{ fontSize: '12px' }}>En Route</span>
                                  )}
                                  {o.status === 'Delivered' && (
                                    <span className="text-muted" style={{ fontSize: '12px' }}>Completed</span>
                                  )}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'dist-stock' && (
                <div>
                  <div className="page-header">
                    <h1>Manage Distributor Local Stock</h1>
                    <p>Adjust current stock availability levels and catalog cost pricing for your warehouse.</p>
                  </div>

                  <div className="card">
                    <div className="table-container">
                      <table>
                        <thead>
                          <tr>
                            <th>Blanket Model</th>
                            <th>Current Inventory Level</th>
                            <th>Cost Price (USD)</th>
                            <th>Last Restocked</th>
                            <th>Actions</th>
                          </tr>
                        </thead>
                        <tbody>
                          {distributorStocks.map(s => (
                            <tr key={s.distributorStockId}>
                              <td><strong>{s.blanketName}</strong></td>
                              <td>
                                <span className={`badge ${s.stockQuantity > 5 ? 'success' : s.stockQuantity > 0 ? 'warning' : 'danger'}`}>
                                  {s.stockQuantity} items
                                </span>
                              </td>
                              <td><strong>${s.costPrice.toFixed(2)}</strong></td>
                              <td>{new Date(s.lastUpdated).toLocaleString()}</td>
                              <td>
                                <button className="btn btn-secondary btn-sm" onClick={() => setEditingStock({
                                  blanketId: s.blanketId,
                                  blanketName: s.blanketName,
                                  stockQuantity: s.stockQuantity,
                                  costPrice: s.costPrice
                                })}>
                                  Edit Stock
                                </button>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {editingStock && (
                    <div className="modal-overlay">
                      <div className="modal">
                        <div className="modal-header">
                          <h3 className="modal-title">Edit Stock - {editingStock.blanketName}</h3>
                          <button className="modal-close" onClick={() => setEditingStock(null)}>×</button>
                        </div>
                        <form onSubmit={handleUpdateStock}>
                          <div className="form-group">
                            <label className="form-label">Available Inventory Quantity</label>
                            <input
                              type="number"
                              min="0"
                              className="form-control"
                              required
                              value={editingStock.stockQuantity}
                              onChange={e => setEditingStock(prev => ({ ...prev, stockQuantity: e.target.value }))}
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Cost Price ($ USD)</label>
                            <input
                              type="number"
                              step="0.01"
                              min="0"
                              className="form-control"
                              required
                              value={editingStock.costPrice}
                              onChange={e => setEditingStock(prev => ({ ...prev, costPrice: e.target.value }))}
                            />
                          </div>
                          <div className="flex gap-2 justify-between mt-4">
                            <button type="button" className="btn btn-secondary" onClick={() => setEditingStock(null)}>Cancel</button>
                            <button type="submit" className="btn btn-primary">Save Changes</button>
                          </div>
                        </form>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'dist-production' && (
                <div>
                  <div className="page-header">
                    <h1>Production Orders</h1>
                    <p>Log of automated back-orders triggered at the manufacturer due to insufficient local stock levels.</p>
                  </div>

                  {distributorProdOrders.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">🏗️</div>
                      <h3>No production back-orders</h3>
                      <p>Back-orders trigger automatically during selling activities if distributor stock is depleted.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Parent Order Reference</th>
                              <th>Blanket Model</th>
                              <th>Quantity</th>
                              <th>Status</th>
                              <th>Factory Queue Date</th>
                              <th>Est. Completion</th>
                            </tr>
                          </thead>
                          <tbody>
                            {distributorProdOrders.map(p => (
                              <tr key={p.productionOrderId}>
                                <td><strong className="text-primary">{p.orderReference}</strong></td>
                                <td>{p.blanketName}</td>
                                <td><strong>{p.quantity} items</strong></td>
                                <td>{getStatusBadge(p.status)}</td>
                                <td>{new Date(p.scheduledDate).toLocaleDateString()}</td>
                                <td>{new Date(p.estimatedCompletionDate).toLocaleDateString()}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'dist-profile' && distributorProfile && (
                <div>
                  <div className="page-header">
                    <h1>Logistics & Costs Configuration</h1>
                    <p>Fine-tune shipping parameters which control the supply chain route calculation algorithms.</p>
                  </div>

                  <div className="card" style={{ maxWidth: '600px' }}>
                    <form onSubmit={handleUpdateProfile}>
                      <div className="form-group">
                        <label className="form-label">Distributor Name</label>
                        <input
                          type="text"
                          className="form-control"
                          required
                          value={distributorProfile.name}
                          onChange={e => setDistributorProfile(prev => ({ ...prev, name: e.target.value }))}
                        />
                      </div>
                      <div className="grid-2">
                        <div className="form-group">
                          <label className="form-label">Contact Email</label>
                          <input
                            type="email"
                            className="form-control"
                            required
                            value={distributorProfile.contactEmail}
                            onChange={e => setDistributorProfile(prev => ({ ...prev, contactEmail: e.target.value }))}
                          />
                        </div>
                        <div className="form-group">
                          <label className="form-label">Contact Phone</label>
                          <input
                            type="text"
                            className="form-control"
                            required
                            value={distributorProfile.contactPhone}
                            onChange={e => setDistributorProfile(prev => ({ ...prev, contactPhone: e.target.value }))}
                          />
                        </div>
                      </div>
                      <div className="grid-2">
                        <div className="form-group">
                          <label className="form-label">Logistics Speed (Transit Days)</label>
                          <input
                            type="number"
                            min="1"
                            className="form-control"
                            required
                            value={distributorProfile.logisticsSpeedDays}
                            onChange={e => setDistributorProfile(prev => ({ ...prev, logisticsSpeedDays: e.target.value }))}
                          />
                        </div>
                        <div className="form-group">
                          <label className="form-label">Flat Shipping Cost ($ USD)</label>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            className="form-control"
                            required
                            value={distributorProfile.shippingCost}
                            onChange={e => setDistributorProfile(prev => ({ ...prev, shippingCost: e.target.value }))}
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-primary btn-lg mt-4" style={{ width: '100%' }}>
                        Save Logistics Metrics
                      </button>
                    </form>
                  </div>
                </div>
              )}
            </>
          )}

          {/* ──────────────────────────────────────────────────────────────────
              MANUFACTURER VIEWS
              ────────────────────────────────────────────────────────────────── */}
          {user.role === 'Manufacturer' && (
            <>
              {activeTab === 'mfg-dashboard' && mfgStats && (
                <div>
                  <div className="page-header">
                    <h1>Manufacturer Dashboard</h1>
                    <p>Cozy Comfort overall factory throughput and distributor success rates.</p>
                  </div>

                  <div className="stat-grid">
                    <div className="stat-card purple">
                      <div className="stat-icon">🛒</div>
                      <div className="stat-value">{mfgStats.totalOrders}</div>
                      <div className="stat-label">Total System Orders</div>
                      <div className="stat-glow" />
                    </div>
                    <div className="stat-card pink">
                      <div className="stat-icon">💰</div>
                      <div className="stat-value">${mfgStats.totalRevenue.toFixed(2)}</div>
                      <div className="stat-label">Delivered Revenue</div>
                      <div className="stat-glow" />
                    </div>
                    <div className="stat-card green">
                      <div className="stat-icon">🏭</div>
                      <div className="stat-value">{mfgStats.awaitingProductionOrders}</div>
                      <div className="stat-label">Queued Production Runs</div>
                      <div className="stat-glow" />
                    </div>
                    <div className="stat-card blue">
                      <div className="stat-icon">🏪</div>
                      <div className="stat-value">{mfgStats.totalSellers}</div>
                      <div className="stat-label">Retail Outlets / Sellers</div>
                      <div className="stat-glow" />
                    </div>
                  </div>

                  <div className="grid-2">
                    <div className="card">
                      <h3 className="mb-4">Logistics Partners Performance</h3>
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Distributor Name</th>
                              <th>Orders Handled</th>
                              <th>Response Speed</th>
                              <th>Delivery success</th>
                            </tr>
                          </thead>
                          <tbody>
                            {mfgStats.distributorPerformance.map((p, idx) => (
                              <tr key={idx}>
                                <td><strong>{p.distributorName}</strong></td>
                                <td>{p.orderCount} orders</td>
                                <td>{p.averageResponseTime} ms</td>
                                <td>
                                  <strong className="text-success">{(p.successRate * 100).toFixed(0)}%</strong>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>

                    <div className="card">
                      <h3 className="mb-4">System Orders by Status</h3>
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Status</th>
                              <th>Orders Count</th>
                            </tr>
                          </thead>
                          <tbody>
                            {mfgStats.ordersByStatus.map((s, idx) => (
                              <tr key={idx}>
                                <td>{getStatusBadge(s.status)}</td>
                                <td><strong>{s.count} orders</strong></td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {activeTab === 'mfg-blankets' && (
                <div>
                  <div className="page-header-row">
                    <div>
                      <h1>Manage Blanket Models</h1>
                      <p>Add and configure catalog items, central factory inventory levels, and production lines.</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setEditingBlanket({
                      name: '', description: '', material: '', size: 'Queen', basePrice: 49.99, imageUrl: '', initialStock: 50, dailyProductionCapacity: 10, leadTimeDays: 2
                    })}>
                      + New Blanket Model
                    </button>
                  </div>

                  <div className="card">
                    <div className="table-container">
                      <table>
                        <thead>
                          <tr>
                            <th>Model Details</th>
                            <th>Material & Size</th>
                            <th>Base Price</th>
                            <th>Warehouse Stock</th>
                            <th>Mfg Capacity / Lead Time</th>
                            <th>Status</th>
                            <th>Actions</th>
                          </tr>
                        </thead>
                        <tbody>
                          {blankets.map(b => (
                            <tr key={b.blanketId}>
                              <td>
                                <div style={{ fontWeight: 700, color: 'var(--text-primary)' }}>{b.name}</div>
                                <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>{b.description}</div>
                              </td>
                              <td>
                                <span className="badge info">{b.material}</span>
                                <span className="badge purple" style={{ marginLeft: '4px' }}>{b.size}</span>
                              </td>
                              <td><strong>${b.basePrice}</strong></td>
                              <td>
                                <span className={`badge ${b.stockQuantity > 10 ? 'success' : 'danger'}`}>
                                  {b.stockQuantity} rolls
                                </span>
                              </td>
                              <td>{b.dailyProductionCapacity}/day | {b.leadTimeDays} days</td>
                              <td>
                                <span className={`badge ${b.isActive ? 'success' : 'muted'}`}>
                                  {b.isActive ? 'Active' : 'Archived'}
                                </span>
                              </td>
                              <td>
                                <div className="flex gap-2">
                                  <button className="btn btn-secondary btn-sm" onClick={() => setEditingBlanket(b)}>Edit Specs</button>
                                  <button className="btn btn-secondary btn-sm" onClick={() => setMfgStockEdit({
                                    blanketId: b.blanketId,
                                    blanketName: b.name,
                                    stockQuantity: b.stockQuantity,
                                    dailyProductionCapacity: b.dailyProductionCapacity,
                                    leadTimeDays: b.leadTimeDays
                                  })}>Edit Stock</button>
                                  <button className="btn btn-danger btn-sm btn-icon" onClick={() => handleDeleteBlanket(b.blanketId)}>🗑️</button>
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {editingBlanket && (
                    <div className="modal-overlay">
                      <div className="modal">
                        <div className="modal-header">
                          <h3 className="modal-title">{editingBlanket.blanketId ? 'Edit Blanket Model' : 'New Blanket Model'}</h3>
                          <button className="modal-close" onClick={() => setEditingBlanket(null)}>×</button>
                        </div>
                        <form onSubmit={handleSaveBlanket}>
                          <div className="form-group">
                            <label className="form-label">Blanket Name</label>
                            <input
                              type="text"
                              className="form-control"
                              placeholder="E.g. Nordic Wool Comforter"
                              required
                              value={editingBlanket.name}
                              onChange={e => setEditingBlanket(prev => ({ ...prev, name: e.target.value }))}
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Description</label>
                            <textarea
                              className="form-control"
                              placeholder="Describe materials, texture, care instructions..."
                              required
                              value={editingBlanket.description}
                              onChange={e => setEditingBlanket(prev => ({ ...prev, description: e.target.value }))}
                            />
                          </div>
                          <div className="grid-2">
                            <div className="form-group">
                              <label className="form-label">Material</label>
                              <input
                                type="text"
                                className="form-control"
                                placeholder="E.g. Merino Wool"
                                required
                                value={editingBlanket.material}
                                onChange={e => setEditingBlanket(prev => ({ ...prev, material: e.target.value }))}
                              />
                            </div>
                            <div className="form-group">
                              <label className="form-label">Size Option</label>
                              <select
                                className="form-control"
                                value={editingBlanket.size}
                                onChange={e => setEditingBlanket(prev => ({ ...prev, size: e.target.value }))}
                              >
                                <option value="Single">Single</option>
                                <option value="Double">Double</option>
                                <option value="Queen">Queen</option>
                                <option value="King">King</option>
                              </select>
                            </div>
                          </div>
                          <div className="grid-2">
                            <div className="form-group">
                              <label className="form-label">Base Retail Price ($ USD)</label>
                              <input
                                type="number"
                                step="0.01"
                                min="0"
                                className="form-control"
                                required
                                value={editingBlanket.basePrice}
                                onChange={e => setEditingBlanket(prev => ({ ...prev, basePrice: parseFloat(e.target.value) }))}
                              />
                            </div>
                            <div className="form-group">
                              <label className="form-label">Image URL</label>
                              <input
                                type="text"
                                className="form-control"
                                placeholder="https://..."
                                value={editingBlanket.imageUrl}
                                onChange={e => setEditingBlanket(prev => ({ ...prev, imageUrl: e.target.value }))}
                              />
                            </div>
                          </div>

                          {!editingBlanket.blanketId && (
                            <div className="card mb-4" style={{ marginTop: '12px' }}>
                              <h4>Initial Factory Setup</h4>
                              <div className="grid-3" style={{ marginTop: '10px' }}>
                                <div className="form-group">
                                  <label className="form-label">Stock Quantity</label>
                                  <input
                                    type="number"
                                    min="0"
                                    className="form-control"
                                    value={editingBlanket.initialStock}
                                    onChange={e => setEditingBlanket(prev => ({ ...prev, initialStock: parseInt(e.target.value) }))}
                                  />
                                </div>
                                <div className="form-group">
                                  <label className="form-label">Daily Capacity</label>
                                  <input
                                    type="number"
                                    min="1"
                                    className="form-control"
                                    value={editingBlanket.dailyProductionCapacity}
                                    onChange={e => setEditingBlanket(prev => ({ ...prev, dailyProductionCapacity: parseInt(e.target.value) }))}
                                  />
                                </div>
                                <div className="form-group">
                                  <label className="form-label">Lead Time (Days)</label>
                                  <input
                                    type="number"
                                    min="1"
                                    className="form-control"
                                    value={editingBlanket.leadTimeDays}
                                    onChange={e => setEditingBlanket(prev => ({ ...prev, leadTimeDays: parseInt(e.target.value) }))}
                                  />
                                </div>
                              </div>
                            </div>
                          )}

                          <div className="flex gap-2 justify-between mt-4">
                            <button type="button" className="btn btn-secondary" onClick={() => setEditingBlanket(null)}>Cancel</button>
                            <button type="submit" className="btn btn-primary">Save Model</button>
                          </div>
                        </form>
                      </div>
                    </div>
                  )}

                  {mfgStockEdit && (
                    <div className="modal-overlay">
                      <div className="modal">
                        <div className="modal-header">
                          <h3 className="modal-title">Edit Stock - {mfgStockEdit.blanketName}</h3>
                          <button className="modal-close" onClick={() => setMfgStockEdit(null)}>×</button>
                        </div>
                        <form onSubmit={handleUpdateMfgStock}>
                          <div className="form-group">
                            <label className="form-label">Warehouse Inventory Rolls</label>
                            <input
                              type="number"
                              min="0"
                              className="form-control"
                              required
                              value={mfgStockEdit.stockQuantity}
                              onChange={e => setMfgStockEdit(prev => ({ ...prev, stockQuantity: e.target.value }))}
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Daily Production Capacity</label>
                            <input
                              type="number"
                              min="1"
                              className="form-control"
                              required
                              value={mfgStockEdit.dailyProductionCapacity}
                              onChange={e => setMfgStockEdit(prev => ({ ...prev, dailyProductionCapacity: e.target.value }))}
                            />
                          </div>
                          <div className="form-group">
                            <label className="form-label">Lead Time Days (Queue Entry Delay)</label>
                            <input
                              type="number"
                              min="1"
                              className="form-control"
                              required
                              value={mfgStockEdit.leadTimeDays}
                              onChange={e => setMfgStockEdit(prev => ({ ...prev, leadTimeDays: e.target.value }))}
                            />
                          </div>
                          <div className="flex gap-2 justify-between mt-4">
                            <button type="button" className="btn btn-secondary" onClick={() => setMfgStockEdit(null)}>Cancel</button>
                            <button type="submit" className="btn btn-primary">Save Stock Metrics</button>
                          </div>
                        </form>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'mfg-orders' && (
                <div>
                  <div className="page-header">
                    <h1>All System Orders</h1>
                    <p>Review orders across the entire supply chain and enforce delivery schedules.</p>
                  </div>

                  {mfgOrders.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">📃</div>
                      <h3>No orders in system</h3>
                      <p>Sellers need to place orders through distributors.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Reference</th>
                              <th>Seller Store</th>
                              <th>Logistics Partner</th>
                              <th>Customer</th>
                              <th>Blanket Model</th>
                              <th>Qty</th>
                              <th>Total Amount</th>
                              <th>Status</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {mfgOrders.map(o => (
                              <tr key={o.orderId}>
                                <td><strong className="text-primary">{o.orderReference}</strong></td>
                                <td>{o.sellerName}</td>
                                <td>{o.distributorName || 'None'}</td>
                                <td>{o.customerName}</td>
                                <td>{o.orderItems.map(oi => oi.blanketName).join(', ')}</td>
                                <td>{o.orderItems.map(oi => oi.quantity).reduce((a, b) => a + b, 0)}</td>
                                <td><strong>${o.totalAmount.toFixed(2)}</strong></td>
                                <td>{getStatusBadge(o.status)}</td>
                                <td>
                                  {o.status === 'Shipped' && (
                                    <button className="btn btn-success btn-sm" onClick={() => handleDeliverOrder(o.orderId)}>
                                      Confirm Delivery
                                    </button>
                                  )}
                                  {o.status === 'Delivered' && (
                                    <span className="text-muted" style={{ fontSize: '12px' }}>Completed</span>
                                  )}
                                  {o.status !== 'Shipped' && o.status !== 'Delivered' && (
                                    <span className="text-muted" style={{ fontSize: '12px' }}>Locked by Partner</span>
                                  )}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'mfg-production' && (
                <div>
                  <div className="page-header">
                    <h1>Factory Production Line</h1>
                    <p>Monitor queued runs triggered by stock discrepancies. Completing a run replenishes partner inventory.</p>
                  </div>

                  {mfgProdOrders.length === 0 ? (
                    <div className="empty-state">
                      <div className="empty-icon">🏭</div>
                      <h3>No active production runs</h3>
                      <p>Factory queues populate automatically when distributor orders exceed local inventory limits.</p>
                    </div>
                  ) : (
                    <div className="card">
                      <div className="table-container">
                        <table>
                          <thead>
                            <tr>
                              <th>Order Ref</th>
                              <th>Blanket Model</th>
                              <th>Fulfilling Partner</th>
                              <th>Batch Quantity</th>
                              <th>Status</th>
                              <th>Est. Completion</th>
                              <th>Queue Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {mfgProdOrders.map(p => (
                              <tr key={p.productionOrderId}>
                                <td><strong className="text-primary">{p.orderReference}</strong></td>
                                <td><strong>{p.blanketName}</strong></td>
                                <td>{p.distributorName}</td>
                                <td><strong>{p.quantity} rolls</strong></td>
                                <td>{getStatusBadge(p.status)}</td>
                                <td>{new Date(p.estimatedCompletionDate).toLocaleDateString()}</td>
                                <td>
                                  {p.status === 'Scheduled' && (
                                    <button className="btn btn-primary btn-sm" onClick={() => handleCompleteProduction(p.productionOrderId)}>
                                      Complete Run & Restock
                                    </button>
                                  )}
                                  {p.status === 'Completed' && (
                                    <span className="text-success" style={{ fontSize: '12px' }}>Rolls Restocked</span>
                                  )}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </>
          )}
        </div>
        <Footer onVisitStore={() => setShowStore(true)} />
      </main>

      {/* Toast notifications */}
      <div className="toast-container">
        {toasts.map(t => (
          <div key={t.id} className={`toast ${t.type === 'error' ? 'error' : 'success'}`}>
            <span>{t.type === 'error' ? '❌' : '✅'}</span>
            <span>{t.message}</span>
          </div>
        ))}
      </div>
    </div>
  )
}

function Footer({ onVisitStore, onStaffPortal }) {
  return (
    <footer className="site-footer">
      <div className="footer-content">
        <div className="footer-section">
          <h3>Cozy Comfort</h3>
          <p>Luxury blankets, seamless distribution, and smart supply chain management.</p>
        </div>
        <div className="footer-section">
          <h4>Navigation</h4>
          <ul>
            {onVisitStore && (
              <li><a href="#" onClick={(e) => { e.preventDefault(); onVisitStore(); }}>🏪 Customer Store</a></li>
            )}
            {onStaffPortal && (
              <li><a href="#" onClick={(e) => { e.preventDefault(); onStaffPortal(); }}>🏢 Staff Portal</a></li>
            )}
          </ul>
        </div>
        <div className="footer-section">
          <h4>Platform Integration</h4>
          <p>Built with React & .NET Core Service-Oriented Architecture.</p>
        </div>
      </div>
      <div className="footer-bottom">
        <p>&copy; {new Date().getFullYear()} Cozy Comfort Inc. All rights reserved.</p>
      </div>
    </footer>
  )
}
