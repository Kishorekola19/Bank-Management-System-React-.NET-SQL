import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { CreditCard, Plus, Lock, Unlock, Eye, EyeOff, ShieldCheck, KeyRound, CheckCircle2, ShieldAlert, Clock } from 'lucide-react';

export const CardsPage = () => {
  const { user } = useAuth();
  const [cards, setCards] = useState([]);
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // View state
  const [showCvv, setShowCvv] = useState({});

  // Request Card Modal
  const [showRequestModal, setShowRequestModal] = useState(false);
  const [reqAccount, setReqAccount] = useState('');
  const [reqCardType, setReqCardType] = useState('VisaDebit');
  const [reqPin, setReqPin] = useState('1234');

  // Change PIN Modal
  const [changePinCard, setChangePinCard] = useState(null);
  const [oldPin, setOldPin] = useState('');
  const [newPin, setNewPin] = useState('');

  const fetchCards = async () => {
    setLoading(true);
    try {
      if (user?.customerId) {
        const res = await api.get(`/atmcard/customer/${user.customerId}`);
        setCards(res.data);

        const accRes = await api.get(`/account/customer/${user.customerId}`);
        const activeAccs = accRes.data.filter(a => a.status === 'Active');
        setAccounts(activeAccs);
        if (activeAccs.length > 0) setReqAccount(activeAccs[0].accountNumber);
      } else if (user?.role === 'Admin') {
        const res = await api.get('/atmcard/all');
        setCards(res.data);
      }
    } catch (err) {
      setError('Failed to load ATM cards.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCards();
  }, [user]);

  const handleRequestCard = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await api.post('/atmcard/request', {
        accountNumber: reqAccount,
        cardType: reqCardType,
        initialPin: reqPin
      });
      setSuccess(`ATM Card application submitted for account ${reqAccount}! Awaiting Admin Approval.`);
      setShowRequestModal(false);
      fetchCards();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to request ATM Card.');
    }
  };

  const handleToggleCardStatus = async (card) => {
    setError('');
    setSuccess('');
    const newStatus = card.status === 'Active' ? 'Blocked' : 'Active';
    try {
      await api.post(`/atmcard/${card.id}/status`, { status: newStatus });
      setSuccess(`ATM Card ${card.cardNumber} has been ${newStatus.toLowerCase()}.`);
      fetchCards();
    } catch (err) {
      setError('Failed to update card status.');
    }
  };

  const handleChangePin = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await api.post(`/atmcard/${changePinCard.id}/pin`, {
        oldPin,
        newPin
      });
      setSuccess(`PIN changed successfully for Card ending in ${changePinCard.cardNumber.slice(-4)}.`);
      setChangePinCard(null);
      setOldPin('');
      setNewPin('');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to change PIN.');
    }
  };

  const toggleShowCvv = (id) => {
    setShowCvv(prev => ({ ...prev, [id]: !prev[id] }));
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
            <CreditCard className="w-7 h-7 text-blue-700" /> {user?.role === 'Admin' ? 'All Issued Virtual Debit Cards' : 'My Virtual ATM Cards'}
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Issued via AtmCardFactory with Admin Sign-Off Workflow
          </p>
        </div>

        {user?.role !== 'Admin' && (
          <button
            onClick={() => setShowRequestModal(true)}
            className="px-4 py-2.5 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow transition-all flex items-center gap-2"
          >
            <Plus className="w-4 h-4" /> Request ATM Card
          </button>
        )}
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-2xl text-red-700 text-sm font-semibold flex items-center gap-2">
          <ShieldAlert className="w-5 h-5 flex-shrink-0" /> {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl text-emerald-800 text-sm font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5 flex-shrink-0" /> {success}
        </div>
      )}

      {/* Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {cards.map((card) => (
          <div key={card.id} className="space-y-3">
            {/* Realistic Bank Debit Card UI */}
            <div className={`relative p-6 rounded-2xl text-white shadow-xl overflow-hidden ${
              card.status === 'Blocked' || card.status === 'PendingApproval'
                ? 'bg-slate-800 border border-slate-700 opacity-90'
                : card.cardType === 'VisaDebit' 
                ? 'bg-gradient-to-br from-blue-900 via-blue-800 to-slate-900 border border-blue-700'
                : card.cardType === 'MastercardDebit'
                ? 'bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 border border-slate-700'
                : 'bg-gradient-to-br from-blue-950 via-slate-900 to-slate-950 border border-blue-900'
            }`}>
              <div className="flex items-center justify-between mb-6">
                <div>
                  <span className="text-xs font-black tracking-widest uppercase text-blue-200">{card.cardType}</span>
                  <span className="block text-[10px] text-slate-300 font-mono">Linked Acc: {card.accountNumber}</span>
                </div>
                <div className="flex items-center gap-2">
                  <span className={`px-2.5 py-0.5 rounded text-[10px] font-black uppercase tracking-wider ${
                    card.status === 'Active' ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-400/40' :
                    card.status === 'PendingApproval' ? 'bg-amber-500/20 text-amber-300 border border-amber-400/40' : 'bg-red-500/20 text-red-300 border border-red-400/40'
                  }`}>
                    {card.status === 'PendingApproval' ? 'Awaiting Approval' : card.status}
                  </span>
                </div>
              </div>

              {/* EMV Chip & Contactless */}
              <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-7 bg-gradient-to-br from-amber-300 to-amber-500 rounded-md border border-amber-200/50 shadow flex items-center justify-center">
                  <div className="w-6 h-4 border border-amber-700/40 rounded-sm"></div>
                </div>
                <div className="text-white/70 text-xs font-mono tracking-widest">)))</div>
              </div>

              {/* Card Number */}
              <div className="font-mono text-lg font-extrabold tracking-widest mb-6 text-white">
                {card.cardNumber}
              </div>

              {/* Card Bottom Details */}
              <div className="flex items-end justify-between">
                <div>
                  <div className="text-[9px] uppercase font-bold text-slate-300">Card Holder</div>
                  <div className="font-bold text-sm tracking-wide text-white uppercase">{card.cardHolderName}</div>
                </div>
                <div className="text-right">
                  <div className="text-[9px] uppercase font-bold text-slate-300">Expires</div>
                  <div className="font-mono font-bold text-sm text-white">{card.expiryDate}</div>
                </div>
                <div className="text-right">
                  <div className="text-[9px] uppercase font-bold text-slate-300">CVV</div>
                  <button
                    onClick={() => toggleShowCvv(card.id)}
                    className="font-mono font-bold text-xs text-amber-300 hover:text-white flex items-center gap-1 justify-end"
                  >
                    {showCvv[card.id] ? card.cvv : '•••'}
                    {showCvv[card.id] ? <EyeOff className="w-3 h-3" /> : <Eye className="w-3 h-3" />}
                  </button>
                </div>
              </div>
            </div>

            {/* Card Controls */}
            <div className="bg-white border border-slate-200 rounded-xl p-3 flex items-center justify-between gap-2 text-xs shadow-sm">
              <div className="text-slate-600 font-mono text-[11px] font-medium">
                Daily Limit: <strong className="text-slate-900">${card.dailyWithdrawalLimit}</strong>
              </div>

              {card.status === 'Active' || card.status === 'Blocked' ? (
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setChangePinCard(card)}
                    className="px-2.5 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-800 rounded-lg font-bold flex items-center gap-1 border border-slate-200"
                  >
                    <KeyRound className="w-3.5 h-3.5" /> PIN
                  </button>
                  <button
                    onClick={() => handleToggleCardStatus(card)}
                    className={`px-2.5 py-1.5 rounded-lg font-bold flex items-center gap-1 border ${
                      card.status === 'Active'
                        ? 'bg-red-50 hover:bg-red-100 text-red-700 border-red-200'
                        : 'bg-emerald-50 hover:bg-emerald-100 text-emerald-800 border-emerald-200'
                    }`}
                  >
                    {card.status === 'Active' ? <Lock className="w-3.5 h-3.5" /> : <Unlock className="w-3.5 h-3.5" />}
                    {card.status === 'Active' ? 'Block' : 'Unblock'}
                  </button>
                </div>
              ) : (
                <span className="text-[11px] font-bold text-amber-700 flex items-center gap-1">
                  <Clock className="w-3 h-3" /> Awaiting Admin Approval
                </span>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Request Card Modal */}
      {showRequestModal && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <CreditCard className="w-5 h-5 text-blue-700" /> Apply for Virtual ATM Debit Card
            </h3>
            <p className="text-xs text-slate-500 font-medium">
              Your request will be submitted to Admin for approval.
            </p>

            <form onSubmit={handleRequestCard} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Link to Bank Account</label>
                <select
                  value={reqAccount}
                  onChange={(e) => setReqAccount(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
                >
                  {accounts.map(acc => (
                    <option key={acc.id} value={acc.accountNumber}>
                      {acc.accountNumber} ({acc.accountType})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Card Network & Tier</label>
                <select
                  value={reqCardType}
                  onChange={(e) => setReqCardType(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                >
                  <option value="VisaDebit">Visa Debit ($2,500 Daily Limit)</option>
                  <option value="MastercardDebit">Mastercard Debit ($3,000 Daily Limit)</option>
                  <option value="PlatinumRuPay">Platinum RuPay ($5,000 Daily Limit)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Set 4-Digit ATM PIN</label>
                <input
                  type="password"
                  required
                  maxLength="4"
                  minLength="4"
                  value={reqPin}
                  onChange={(e) => setReqPin(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
                  placeholder="1234"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowRequestModal(false)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow-md"
                >
                  Submit Request
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Change PIN Modal */}
      {changePinCard && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <KeyRound className="w-5 h-5 text-blue-700" /> Change ATM Card PIN
            </h3>
            <div className="text-xs text-slate-600 font-mono font-medium">
              Card Number: {changePinCard.cardNumber}
            </div>

            <form onSubmit={handleChangePin} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Current 4-Digit PIN</label>
                <input
                  type="password"
                  required
                  maxLength="4"
                  minLength="4"
                  value={oldPin}
                  onChange={(e) => setOldPin(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
                  placeholder="••••"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">New 4-Digit PIN</label>
                <input
                  type="password"
                  required
                  maxLength="4"
                  minLength="4"
                  value={newPin}
                  onChange={(e) => setNewPin(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
                  placeholder="••••"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setChangePinCard(null)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow-md"
                >
                  Update PIN
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
