import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { Building2, LayoutDashboard, CreditCard, ArrowRightLeft, Landmark, ShieldCheck, LogOut, Users, FileCheck } from 'lucide-react';

export const Navbar = ({ activeTab, setActiveTab }) => {
  const { user, logout } = useAuth();
  const [pendingCount, setPendingCount] = useState(0);

  useEffect(() => {
    if (user?.role === 'Admin') {
      const checkPending = async () => {
        try {
          const res = await api.get('/adminapprovals/pending-requests');
          const count = (res.data.pendingAccountOpenings?.length || 0) +
                        (res.data.pendingAccountClosures?.length || 0) +
                        (res.data.pendingCards?.length || 0) +
                        (res.data.pendingLoans?.length || 0);
          setPendingCount(count);
        } catch (err) {
          console.error(err);
        }
      };
      checkPending();
      const interval = setInterval(checkPending, 10000);
      return () => clearInterval(interval);
    }
  }, [user]);

  let navItems = [];

  if (user?.role === 'Admin') {
    navItems = [
      { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
      { id: 'approvals', label: 'Pending Approvals', icon: FileCheck, badge: pendingCount },
      { id: 'accounts', label: 'All Accounts', icon: CreditCard },
      { id: 'cards', label: 'ATM Cards', icon: CreditCard },
      { id: 'loans', label: 'Loans', icon: Landmark },
      { id: 'customers', label: 'Customers', icon: Users },
      { id: 'audit', label: 'Audit Logs', icon: ShieldCheck },
    ];
  } else {
    navItems = [
      { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
      { id: 'accounts', label: 'Accounts', icon: CreditCard },
      { id: 'cards', label: 'ATM Cards', icon: CreditCard },
      { id: 'transfer', label: 'Fund Transfer', icon: ArrowRightLeft },
      { id: 'loans', label: 'Loans & EMI', icon: Landmark },
    ];
  }

  return (
    <header className="bg-slate-900 border-b border-slate-800 sticky top-0 z-50 text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <div className="flex items-center gap-3 cursor-pointer" onClick={() => setActiveTab('dashboard')}>
            <div className="p-2 bg-blue-700 rounded-xl shadow">
              <Building2 className="w-6 h-6 text-white" />
            </div>
            <div>
              <span className="font-bold text-lg tracking-tight text-white">
                Apex Enterprise Bank
              </span>
              <span className="block text-[10px] text-slate-400 font-mono tracking-wider">
                {user?.role === 'Admin' ? 'ADMIN MANAGEMENT PORTAL' : 'CUSTOMER ONLINE BANKING'}
              </span>
            </div>
          </div>

          <nav className="hidden md:flex space-x-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = activeTab === item.id;
              return (
                <button
                  key={item.id}
                  onClick={() => setActiveTab(item.id)}
                  className={`flex items-center gap-2 px-3.5 py-2 rounded-lg text-sm font-semibold transition-all relative ${
                    isActive
                      ? 'bg-blue-700 text-white shadow-sm'
                      : 'text-slate-300 hover:text-white hover:bg-slate-800'
                  }`}
                >
                  <Icon className="w-4 h-4" />
                  {item.label}
                  {item.badge > 0 && (
                    <span className="ml-1 px-1.5 py-0.2 bg-amber-500 text-slate-950 font-extrabold text-[10px] rounded-full animate-pulse">
                      {item.badge}
                    </span>
                  )}
                </button>
              );
            })}
          </nav>

          <div className="flex items-center gap-4">
            <div className="text-right hidden sm:block">
              <div className="text-sm font-bold text-white">{user?.fullName}</div>
              <div className="text-xs text-slate-400 flex items-center justify-end gap-1 font-mono">
                <span className={`inline-block w-2 h-2 rounded-full ${user?.role === 'Admin' ? 'bg-amber-400' : 'bg-emerald-400'}`}></span>
                {user?.role === 'Admin' ? 'System Administrator' : `Customer (#${user?.customerId || 'N/A'})`}
              </div>
            </div>

            <button
              onClick={logout}
              title="Logout"
              className="p-2 rounded-lg bg-slate-800 hover:bg-red-600/80 text-slate-300 hover:text-white transition-colors border border-slate-700"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        </div>
      </div>

      {/* Mobile nav bar */}
      <div className="md:hidden flex border-t border-slate-800 overflow-x-auto px-2 py-1 bg-slate-900">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.id;
          return (
            <button
              key={item.id}
              onClick={() => setActiveTab(item.id)}
              className={`flex-1 flex flex-col items-center py-2 px-1 text-xs font-medium relative ${
                isActive ? 'text-blue-400 font-bold' : 'text-slate-400'
              }`}
            >
              <Icon className="w-4 h-4 mb-1" />
              {item.label}
              {item.badge > 0 && (
                <span className="absolute top-1 right-2 w-2 h-2 bg-amber-500 rounded-full"></span>
              )}
            </button>
          );
        })}
      </div>
    </header>
  );
};
