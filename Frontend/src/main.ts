import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { bootstrapApplication } from '@angular/platform-browser';

interface UserResponse {
  id: number;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  role: string;
  specialization?: string;
  gender: string;
  age: number;
  createdAtUtc: string;
}

interface Appointment {
  id?: number;
  userId: number;
  doctorId?: number | null;
  patientName: string;
  problemType: string;
  time: string;
  status: string;
}

interface CalendarDay {
  date: Date;
  dateStr: string;
  dayNum: number;
  hasAppointments: boolean;
  isToday: boolean;
  isPast: boolean;
}

interface CalendarMonth {
  year: number;
  month: number;
  label: string;
  weeks: (CalendarDay | null)[][];
}

const PROBLEM_TYPES: { label: string; specialization: string }[] = [
  { label: 'General Checkup',        specialization: 'General Practice' },
  { label: 'Heart Problems',         specialization: 'Cardiology' },
  { label: 'Neurology',              specialization: 'Neurology' },
  { label: 'Bone & Joint Problems',  specialization: 'Orthopedics' },
  { label: 'Skin Problems',          specialization: 'Dermatology' },
  { label: 'Urology',                specialization: 'Urology' },
  { label: "Women's Health",         specialization: 'Gynecology' },
  { label: 'Digestive Problems',     specialization: 'Gastroenterology' },
  { label: 'Child Health',           specialization: 'Pediatrics' },
  { label: 'Eye Problems',           specialization: 'Ophthalmology' },
  { label: 'Ear, Nose & Throat',     specialization: 'ENT' },
  { label: 'Mental Health',          specialization: 'Psychiatry' },
];

const MONTH_NAMES = ['January','February','March','April','May','June',
                     'July','August','September','October','November','December'];
const DAY_LABELS = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];

// 30-minute slots from 08:00 to 17:30
const TIME_SLOTS: { label: string; value: string }[] = [];
for (let h = 8; h <= 17; h++) {
  for (const m of [0, 30]) {
    if (h === 17 && m === 30) continue;
    const hh  = String(h).padStart(2, '0');
    const mm  = String(m).padStart(2, '0');
    const ampm = h < 12 ? 'AM' : 'PM';
    const h12 = h > 12 ? h - 12 : h === 0 ? 12 : h;
    TIME_SLOTS.push({ label: `${h12}:${mm} ${ampm}`, value: `${hh}:${mm}` });
  }
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './app/app.component.html',
  styleUrl: './app/app.component.css'
})
class AppComponent {
  private readonly http = inject(HttpClient);
  private readonly userApi        = 'http://localhost:7284/api/User';
  private readonly appointmentApi = 'http://localhost:7104/api/Appointment';

  readonly problemTypes = PROBLEM_TYPES;
  readonly dayLabels    = DAY_LABELS;
  readonly timeSlots    = TIME_SLOTS;

  mode: 'login' | 'register' = 'login';
  currentUser: UserResponse | null = null;
  message = '';
  error   = '';

  registerModel = {
    userId: '', firstName: '', lastName: '',
    email: '', password: '', role: 'Patient',
    specialization: '', gender: '', age: null as number | null
  };

  loginModel = { userId: '', password: '' };

  appointmentModel = {
    problemType: '',
    doctorId: null as number | null,
    date: '',
    time: ''
  };

  availableDoctors: UserResponse[]  = [];
  appointments: Appointment[]       = [];
  doctorNameMap: Record<number, string> = {};
  calendarMonths: CalendarMonth[]   = [];
  readonly today = new Date().toISOString().split('T')[0];
  pendingDateStr: string | null     = null;   // date clicked but not yet confirmed
  selectedDateStr: string | null    = null;   // date confirmed with OK
  selectedDayAppointments: Appointment[] = [];
  confirmingCancelId: number | null = null;

  get isDoctor(): boolean { return this.currentUser?.role === 'Doctor'; }

  switchMode(mode: 'login' | 'register'): void {
    this.mode = mode; this.message = ''; this.error = '';
  }

  register(): void {
    this.clearStatus();
    this.http.post<UserResponse>(this.userApi, this.registerModel).subscribe({
      next: user => { this.currentUser = user; this.message = 'Registration successful.'; this.afterLogin(); },
      error: err  => this.error = this.getError(err)
    });
  }

  login(): void {
    this.clearStatus();
    this.http.post<UserResponse>(`${this.userApi}/login`, this.loginModel).subscribe({
      next: user => { this.currentUser = user; this.message = 'Login successful.'; this.afterLogin(); },
      error: err  => this.error = this.getError(err)
    });
  }

  private afterLogin(): void {
    this.loadAppointments();
    if (this.isDoctor) this.buildCalendar([]);
  }

  onProblemTypeChange(): void {
    this.appointmentModel.doctorId = null;
    this.availableDoctors = [];
    const selected = this.problemTypes.find(p => p.label === this.appointmentModel.problemType);
    if (!selected) return;
    this.http.get<UserResponse[]>(`${this.userApi}/doctors?specialization=${encodeURIComponent(selected.specialization)}`).subscribe({
      next: docs => this.availableDoctors = docs,
      error: err  => this.error = this.getError(err)
    });
  }

  createAppointment(): void {
    if (!this.currentUser) return;
    this.clearStatus();

    if (!this.appointmentModel.problemType) {
      this.error = 'Please select a problem type.';
      return;
    }
    if (!this.appointmentModel.doctorId) {
      this.error = 'Please select a doctor.';
      return;
    }
    if (!this.appointmentModel.date || !this.appointmentModel.time) {
      this.error = 'Please select a date and time.';
      return;
    }
    const appt: Appointment = {
      userId:      this.currentUser.id,
      doctorId:    this.appointmentModel.doctorId,
      patientName: this.currentUser.fullName,
      problemType: this.appointmentModel.problemType,
      time:        `${this.appointmentModel.date}T${this.appointmentModel.time}:00`,
      status:      'Pending'
    };
    this.http.post<Appointment>(this.appointmentApi, appt).subscribe({
      next: created => {
        this.message = `Appointment #${created.id} created — awaiting doctor confirmation.`;
        this.appointmentModel = { problemType: '', doctorId: null, date: '', time: '' };
        this.availableDoctors = [];
        this.loadAppointments();
      },
      error: err => this.error = this.getError(err)
    });
  }

  approve(appt: Appointment): void {
    this.http.patch<Appointment>(`${this.appointmentApi}/${appt.id}/approve`, {}).subscribe({
      next: updated => {
        appt.status = updated.status;
        this.message = `Appointment #${appt.id} confirmed.`;
        if (this.selectedDateStr) this.selectDay(this.selectedDateStr);
      },
      error: err => this.error = this.getError(err)
    });
  }

  requestCancel(appt: Appointment): void {
    this.confirmingCancelId = appt.id ?? null;
    this.clearStatus();
  }

  dismissCancel(): void {
    this.confirmingCancelId = null;
  }

  confirmCancel(appt: Appointment): void {
    this.confirmingCancelId = null;
    this.http.patch<Appointment>(`${this.appointmentApi}/${appt.id}/cancel`, {}).subscribe({
      next: updated => {
        // update in all local arrays so both views reflect the change immediately
        const updateStatus = (a: Appointment) => {
          if (a.id === updated.id) a.status = updated.status;
        };
        this.appointments.forEach(updateStatus);
        this.selectedDayAppointments.forEach(updateStatus);
        this.message = `Appointment #${appt.id} for ${appt.patientName} has been cancelled.`;
      },
      error: err => this.error = this.getError(err)
    });
  }

  refresh(): void {
    this.clearStatus();
    this.appointmentModel = { problemType: '', doctorId: null, date: '', time: '' };
    this.availableDoctors = [];
    this.pendingDateStr = null;
    this.selectedDateStr = null;
    this.selectedDayAppointments = [];
    this.confirmingCancelId = null;
    this.loadAppointments();
  }

  loadAppointments(): void {
    if (!this.currentUser) return;
    if (this.isDoctor) {
      this.http.get<Appointment[]>(`${this.appointmentApi}/doctor/${this.currentUser.id}`).subscribe({
        next: appts => { this.appointments = appts; this.buildCalendar(appts); },
        error: err   => this.error = this.getError(err)
      });
    } else {
      this.http.get<Appointment[]>(this.appointmentApi).subscribe({
        next: appts => {
          this.appointments = appts
            .filter(a => a.userId === this.currentUser?.id)
            .sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime());
          this.resolveDoctorNames(this.appointments);
        },
        error: err => this.error = this.getError(err)
      });
    }
  }

  private resolveDoctorNames(appts: Appointment[]): void {
    const ids = [...new Set(appts.map(a => a.doctorId).filter((id): id is number => id != null))];
    ids.forEach(id => {
      if (this.doctorNameMap[id]) return;
      this.http.get<UserResponse>(`${this.userApi}/${id}`).subscribe({
        next: user => this.doctorNameMap[id] = user.fullName,
        error: ()  => this.doctorNameMap[id] = `Doctor #${id}`
      });
    });
  }

  getDoctorName(doctorId: number | null | undefined): string {
    if (doctorId == null) return '—';
    return this.doctorNameMap[doctorId] ?? '—';
  }

  // ── Calendar ────────────────────────────────────────────────

  buildCalendar(appts: Appointment[]): void {
    const apptDates = new Set(appts.map(a => a.time.substring(0, 10)));
    const today     = new Date();
    today.setHours(0, 0, 0, 0);
    const todayStr  = this.toDateStr(today);

    this.calendarMonths = [];
    for (let m = 0; m < 3; m++) {
      const base   = new Date(today.getFullYear(), today.getMonth() + m, 1);
      const year   = base.getFullYear();
      const month  = base.getMonth();
      const label  = `${MONTH_NAMES[month]} ${year}`;
      const daysInMonth  = new Date(year, month + 1, 0).getDate();
      const firstWeekday = new Date(year, month, 1).getDay();

      const allDays: (CalendarDay | null)[] = Array(firstWeekday).fill(null);
      for (let d = 1; d <= daysInMonth; d++) {
        const date    = new Date(year, month, d);
        const dateStr = this.toDateStr(date);
        allDays.push({
          date, dateStr, dayNum: d,
          hasAppointments: apptDates.has(dateStr),
          isToday: dateStr === todayStr,
          isPast:  date < today
        });
      }
      while (allDays.length % 7 !== 0) allDays.push(null);

      const weeks: (CalendarDay | null)[][] = [];
      for (let i = 0; i < allDays.length; i += 7) weeks.push(allDays.slice(i, i + 7));

      this.calendarMonths.push({ year, month, label, weeks });
    }
  }

  pickDay(dateStr: string): void {
    this.pendingDateStr = dateStr;
    this.selectedDateStr = null;
    this.selectedDayAppointments = [];
    this.confirmingCancelId = null;
  }

  confirmDay(): void {
    if (!this.pendingDateStr) return;
    this.selectedDateStr = this.pendingDateStr;
    this.pendingDateStr = null;
    this.selectedDayAppointments = this.appointments
      .filter(a => a.time.startsWith(this.selectedDateStr!))
      .sort((a, b) => a.time.localeCompare(b.time));
  }

  selectDay(dateStr: string): void {
    this.selectedDayAppointments = this.appointments
      .filter(a => a.time.startsWith(dateStr))
      .sort((a, b) => a.time.localeCompare(b.time));
  }

  clearSelection(): void {
    this.selectedDateStr = null;
    this.pendingDateStr = null;
    this.selectedDayAppointments = [];
  }

  monthPrefix(month: CalendarMonth): string {
    return `${month.year}-${String(month.month + 1).padStart(2, '0')}`;
  }

  private toDateStr(d: Date): string {
    const y  = d.getFullYear();
    const mo = String(d.getMonth() + 1).padStart(2, '0');
    const dy = String(d.getDate()).padStart(2, '0');
    return `${y}-${mo}-${dy}`;
  }

  logout(): void {
    this.currentUser = null; this.appointments = []; this.availableDoctors = [];
    this.calendarMonths = []; this.pendingDateStr = null; this.selectedDateStr = null;
    this.selectedDayAppointments = []; this.confirmingCancelId = null;
    this.message = ''; this.error = '';
  }

  private clearStatus(): void { this.message = ''; this.error = ''; }

  private getError(err: unknown): string {
    if (typeof err === 'object' && err !== null) {
      const e = err as { status?: number; error?: unknown; message?: string };
      if (typeof e.error === 'string' && e.error.length) return `${e.status}: ${e.error}`;
      if (typeof e.error === 'object' && e.error !== null) return `${e.status}: ${JSON.stringify(e.error)}`;
      if (typeof e.message === 'string') return e.message;
    }
    return 'Something went wrong. Check that the APIs are running.';
  }
}

bootstrapApplication(AppComponent).catch(e => console.error(e));
